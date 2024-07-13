namespace PathfinderFr.Markup.WikiFormatter
{
    internal struct NamespaceInfo
    {
        public static readonly NamespaceInfo Empty = new NamespaceInfo();

        public NamespaceInfo(string ns)
        {
            Name = ns;
        }

        public string Name { get; }

        public override bool Equals(object obj)
        {
            if (obj is NamespaceInfo other) return Name == other.Name;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (Name != null) return Name.GetHashCode();
            return base.GetHashCode();
        }

        public static bool operator ==(NamespaceInfo ns1, NamespaceInfo ns2) => ns1.Name == ns2.Name;

        public static bool operator !=(NamespaceInfo ns1, NamespaceInfo ns2) => ns1.Name != ns2.Name;
    }

}
