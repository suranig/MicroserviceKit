namespace OrderService.UnitTests.Utilities;

public static class TestData
{
    public static class Guids
    {
        public static readonly Guid Valid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        public static readonly Guid Another = Guid.Parse("87654321-4321-4321-4321-210987654321");
        public static readonly Guid Empty = Guid.Empty;
    }

    public static class Strings
    {
        public const string Valid = "Test String";
        public const string Long = "This is a very long string that might be used for testing maximum length validations and other string-related scenarios";
        public const string Empty = "";
        public const string WhiteSpace = "   ";
    }

    public static class Numbers
    {
        public const int ValidInt = 42;
        public const int Zero = 0;
        public const int Negative = -1;
        public const decimal ValidDecimal = 123.45m;
        public const decimal ZeroDecimal = 0m;
        public const decimal NegativeDecimal = -123.45m;
    }

    public static class Dates
    {
        public static readonly DateTime Valid = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        public static readonly DateTime Past = new(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        public static readonly DateTime Future = new(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    }
}