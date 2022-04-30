using System;
using FluentAssertions;
using Xunit;

namespace TubeScan.Tests
{
    public class ExtensionsTests
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void ToReturnCode_IsPositiveValueOrNot(bool rc, bool isPositive)
        {
            var r = rc.ToReturnCode();

            if (isPositive)
                r.Should().BeGreaterThan(0);
            else
                r.Should().Be(0);
        }

        [Fact]
        public void ArgNotNull_NotNull_ReturnsValue()
        {
            string s = "abc";
            var r = s.ArgNotNull(nameof(s));

            r.Should().Be(s);
        }

        [Fact]
        public void InvalidOpArg_Invalid_ThrowsException()
        {
            string s = null;

            Action a = () => s.InvalidOpArg(x => false, "Invalid");

            a.Should().Throw<InvalidOperationException>().WithMessage("?*");
        }

        [Fact]
        public void InvalidOpArg_NullPredicate_ThrowsException()
        {
            Action a = () => 1.InvalidOpArg(null, "invalid");

            a.Should().Throw<ArgumentNullException>().WithMessage("?*");
        }

        [Fact]
        public void InvalidOpArg_NoError_ReturnsValue()
        {
            var value = 1234;
            var r = value.InvalidOpArg(x => false, "invalid");

            r.Should().Be(value);

        }

        [Fact]
        public void ArgNotNull_Null_ThrowsException()
        {
            string s = null;

            Action a = () => s.ArgNotNull(nameof(s));

            a.Should().Throw<ArgumentNullException>().WithParameterName(nameof(s));
        }

        [Theory]
        [InlineData(2022, 1,1, 13, 13)]
        [InlineData(2022, 6, 1, 13, 14)]
        public void ToUkDateTime_TimeConverted(int year, int month, int day, int hour, int expectedHour)
        {
            var now = new DateTime(year, month, day, hour, 0, 0, DateTimeKind.Utc);

            var uk = now.ToUkDateTime();
            var ukdto = new DateTimeOffset(uk);

            ukdto.Hour.Should().Be(expectedHour);
        }

#pragma warning restore CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}