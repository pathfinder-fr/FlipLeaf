using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace FlipLeaf
{
    class YamlHeaderRenderer : IRenderingMiddleware
    {
        public void Render(RenderContext context, Action<RenderContext> next)
        {
            var input = new StreamReader(context.Input);
            var deserializer = new DeserializerBuilder().Build();
            var parser = new Parser(input);

            int i;
            parser.Expect<StreamStart>();

            if (parser.Accept<DocumentStart>())
            {

                var doc = deserializer.Deserialize(parser);
                context.Values["PageData"] = ConvertDoc(doc);
                
                if (parser.Accept<DocumentStart>())
                {
                    i = parser.Current.End.Index - 1;
                    char c;

                    do
                    {
                        i++;
                        c = (char)input.Read();
                    } while (c == '\r' || c == '\n');
                    
                }

                context.Input = new SubStream(context.Input);
            }


            next?.Invoke(context);
        }

        class SubStream : Stream
        {
            private readonly Stream parent;
            private readonly long _offset;

            public SubStream(Stream parent)
            {
                this.parent = parent;
                this._offset = parent.Position;
            }

            public override void Flush() => this.parent.Flush();

            public override int Read(byte[] buffer, int offset, int count) => this.parent.Read(buffer, offset, count);

            public override long Seek(long offset, SeekOrigin origin) => this.parent.Seek(offset + this._offset, origin) - _offset;

            public override void SetLength(long value) => this.parent.SetLength(value + this._offset);

            public override void Write(byte[] buffer, int offset, int count) => this.parent.Write(buffer, offset, count);

            public override bool CanRead => this.parent.CanRead;
            public override bool CanSeek => this.parent.CanSeek;
            public override bool CanWrite => this.parent.CanWrite;
            public override long Length => this.parent.Length;

            public override long Position
            {
                get => this.parent.Position - this._offset;
                set => this.parent.Position = value + this._offset;
            }
        }

        private static object ConvertDoc(object doc)
        {
            var docType = doc.GetType();

            switch (Type.GetTypeCode(docType))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Empty:
                    return doc;

                case TypeCode.Object:
                    if (doc == null)
                        return doc;

                    switch (doc)
                    {
                        case IDictionary<object, object> objectDict:
                            return objectDict.ToDictionary(p => p.Key.ToString(), p => ConvertDoc(p.Value));

                        case IList<object> objectList:
                            return objectList.Select(ConvertDoc).ToList();
                        default:
                            return doc;
                    }

                default:
                    return doc;
            }
        }
    }
}
