namespace MatrixWoutSIMD
{
    public class Element
    {
        public float Value { get; set; }

        public float Row { get; set; }

        public float Column { get; set; }

        public override string ToString()
        {
            return $"Value: {Value}; Row: {Row}; Column: {Column}; ";
        }
    }
}
