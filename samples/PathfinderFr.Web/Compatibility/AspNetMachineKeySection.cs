using System.Security.Cryptography;

namespace PathfinderFr.Compatibility
{
    public sealed class AspNetMachineKeySection : IDisposable
    {
        private readonly int _IVLengthDecryption;
        private readonly SymmetricAlgorithm _symAlgoDecryption;
        private readonly KeyedHashAlgorithm _hashAlgoValidation;

        public AspNetMachineKeySection(string decryptionAlg, string decryptionKey, string validationAlg, string validationKey)
            : this(decryptionAlg, HexToBytes(decryptionKey), validationAlg, HexToBytes(validationKey))
        {
        }

        public AspNetMachineKeySection(string decryptionAlg, byte[] decryptionKey, string validationAlg, byte[] validationKey)
        {
            switch (validationAlg)
            {
                case "SHA1":
                    KeySize = 20;
                    _hashAlgoValidation = new HMACSHA1();
                    break;

                default:
                    throw new NotSupportedException();
            }

            _hashAlgoValidation.Key = validationKey;

            _symAlgoDecryption = decryptionAlg switch
            {
                "AES" => new AesCryptoServiceProvider(),
                _ => throw new NotSupportedException(),
            };

            _symAlgoDecryption.Key = decryptionKey;
            _symAlgoDecryption.GenerateIV();
            _IVLengthDecryption = RoundupNumBitsToNumBytes(_symAlgoDecryption.KeySize);
        }

        public int KeySize { get; }

        public void Dispose()
        {
            _symAlgoDecryption.Dispose();
            _hashAlgoValidation.Dispose();
        }

        public byte[] DecryptData(string hexData)
        {
            return DecryptData(HexToBytes(hexData));
        }

        public byte[] DecryptData(byte[] buf)
        {
            var length = buf.Length;
            var start = 0;

            //try
            {
                //EnsureConfig();
                //if (!fEncrypt && signData)
                {
                    if (start != 0 || length != buf.Length)
                    {
                        var array = new byte[length];
                        Buffer.BlockCopy(buf, start, array, 0, length);
                        buf = array;
                        start = 0;
                    }

                    buf = GetUnHashedData(buf);

                    if (buf == null)
                    {
                        throw new InvalidOperationException("Unable_to_validate_data");
                    }

                    length = buf.Length;
                }

                byte[] bData;
                using (var ms = new MemoryStream())
                using (var cryptoTransform = _symAlgoDecryption.CreateDecryptor())//GetCryptoTransform(fEncrypt, useValidationSymAlgo, useLegacyMode);)
                using (var cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
                {
                    //bool createIV = signData || (ivType != 0 && CompatMode > MachineKeyCompatibilityMode.Framework20SP1);
                    //if (fEncrypt && createIV)
                    //{
                    //    //int ivLength = useValidationSymAlgo ? _IVLengthValidation : _IVLengthDecryption;
                    //    int ivLength = _IVLengthDecryption;
                    //    byte[] iv = null;

                    //    // IVType = Random
                    //    iv = new byte[ivLength];
                    //    RandomNumberGenerator.GetBytes(iv);

                    //    cs.Write(iv, 0, iv.Length);
                    //}

                    cs.Write(buf, start, length);
                    //if (fEncrypt && modifier != null)
                    //{
                    //    cs.Write(modifier, 0, modifier.Length);
                    //}

                    cs.FlushFinalBlock();
                    var paddedData = ms.ToArray();

                    // At this point:
                    // If fEncrypt = true (encrypting), paddedData := Enc(iv + buf + modifier)
                    // If fEncrypt = false (decrypting), paddedData := iv + plaintext + modifier
                    cs.Close();

                    // ReturnCryptoTransform(fEncrypt, cryptoTransform, useValidationSymAlgo, useLegacyMode);
                    cryptoTransform.Dispose();

                    // DevDiv Bugs 137864: Strip IV from beginning of unencrypted data
                    //if (!fEncrypt && createIV)
                    {
                        // strip off the first bytes that were random bits
                        //int ivLength = useValidationSymAlgo ? _IVLengthValidation : _IVLengthDecryption;
                        var ivLength = _IVLengthDecryption;

                        var bDataLength = paddedData.Length - ivLength;
                        if (bDataLength < 0)
                        {
                            throw new InvalidOperationException("Unable_to_validate_data");
                        }

                        bData = new byte[bDataLength];
                        Buffer.BlockCopy(paddedData, ivLength, bData, 0, bDataLength);
                    }
                    //else
                    //{
                    //    bData = paddedData;
                    //}
                }

                // At this point:
                // If fEncrypt = true (encrypting), bData := Enc(iv + buf + modifier)
                // If fEncrypt = false (decrypting), bData := plaintext + modifier

                //if (!fEncrypt && modifier != null && modifier.Length != 0)
                //{
                //    // Compare the end of bData with the expected modifier to validate they are the same
                //    if (!BuffersAreEqual(bData, bData.Length - modifier.Length, modifier.Length, modifier, 0, modifier.Length))
                //    {
                //        throw new InvalidOperationException("Unable_to_validate_data");
                //    }

                //    byte[] bData2 = new byte[bData.Length - modifier.Length];
                //    Buffer.BlockCopy(bData, 0, bData2, 0, bData2.Length);
                //    bData = bData2;
                //}

                // At this point:
                // If fEncrypt = true (encrypting), bData := Enc(iv + buf + modifier)
                // If fEncrypt = false (decrypting), bData := plaintext

                //if (fEncrypt && signData)
                //{
                //    byte[] hmac = HashData(bData, 0, bData.Length);
                //    byte[] bData2 = new byte[bData.Length + hmac.Length];
                //    Buffer.BlockCopy(bData, 0, bData2, 0, bData.Length);
                //    Buffer.BlockCopy(hmac, 0, bData2, bData.Length, hmac.Length);
                //    bData = bData2;
                //}

                // At this point:
                // If fEncrypt = true (encrypting), bData := Enc(iv + buf + modifier) + HMAC(Enc(iv + buf + modifier))
                // If fEncrypt = false (decrypting), bData := plaintext

                // And we're done
                return bData;
            }

        }

        public bool VerifyHashedData(byte[] bufHashed)
        {
            //////////////////////////////////////////////////////////////////////
            // Step 1: Get the MAC: Last [HashSize] bytes
            if (bufHashed.Length <= KeySize)
            {
                return false;
            }

            var bMac = HashData(bufHashed, 0, bufHashed.Length - KeySize);

            //////////////////////////////////////////////////////////////////////
            // Step 2: Make sure the MAC has expected length
            if (bMac == null || bMac.Length != KeySize)
            {
                return false;
            }

            var lastPos = bufHashed.Length - KeySize;

            return BuffersAreEqual(bMac, 0, KeySize, bufHashed, lastPos, KeySize);
        }

        private static int RoundupNumBitsToNumBytes(int numBits)
        {
            if (numBits < 0)
            {
                return 0;
            }

            return numBits / 8 + (((numBits & 7) != 0) ? 1 : 0);
        }


        private byte[] GetUnHashedData(byte[] bufHashed)
        {
            if (!VerifyHashedData(bufHashed))
            {
                return null;
            }

            var array = new byte[bufHashed.Length - KeySize];
            Buffer.BlockCopy(bufHashed, 0, array, 0, array.Length);
            return array;
        }

        private byte[] HashData(byte[] buf, int start, int length)
        {
            if (start < 0 || start > buf.Length)
            {
                throw new ArgumentException("InvalidArgumentValue", "start");
            }

            if (length < 0 || buf == null || start + length > buf.Length)
            {
                throw new ArgumentException("InvalidArgumentValue", "length");
            }

            var hash = _hashAlgoValidation.ComputeHash(buf, start, length);

            if (hash == null)
            {
                throw new NotSupportedException();
            }

            if (hash.Length != KeySize)
            {
                throw new NotSupportedException();
            }

            return hash;
        }

        private static bool BuffersAreEqual(byte[] buffer1, int buffer1Offset, int buffer1Count, byte[] buffer2, int buffer2Offset, int buffer2Count)
        {
            if (buffer1Count != buffer2Count)
            {
                return false;
            }

            var success = 0;
            unchecked
            {
                for (var i = 0; i < buffer1Count; i++)
                {
                    success |= (buffer1[buffer1Offset + i] - buffer2[buffer2Offset + i]);
                }
            }
            return (0 == success);
        }

        private static byte[] HexToBytes(string hex)
        {
            var valueBytes = new byte[hex.Length / 2];
            for (var i = 0; i < valueBytes.Length; i++)
            {
                valueBytes[i] = byte.Parse(hex.AsSpan(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }

            return valueBytes;
        }
    }
}
