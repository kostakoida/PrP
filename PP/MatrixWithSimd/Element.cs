namespace MatrixWithSIMD
{
    public class Element
    {
        public float Value { get; set; }

        public int Row { get; set; }

        public int Column { get; set; }

        public override string ToString()
        {
            return $"Value: {Value}; Row: {Row}; Column: {Column}; ";
        }
    }
}
