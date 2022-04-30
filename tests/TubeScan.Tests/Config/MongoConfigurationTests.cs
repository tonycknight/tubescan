using FluentAssertions;
using TubeScan.Config;
using Xunit;

namespace TubeScan.Tests.Config
{
    public class MongoConfigurationTests
    {
        [Fact]
        public void DatabaseName_DefaultExists()
        {
            var c = new MongoConfiguration();

            c.DatabaseName.Should().NotBeNullOrWhiteSpace();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(" a ")]
        public void DatabaseName_ValueApplied(string value)
        {
            var c = new MongoConfiguration()
            {
                DatabaseName = value,
            };

            c.DatabaseName.Should().Be(value);
        }


        //

        [Fact]
        public void StationTagsCollectionName_DefaultExists()
        {
            var c = new MongoConfiguration();

            c.StationTagsCollectionName.Should().NotBeNullOrWhiteSpace();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(" a ")]
        public void StationTagsCollectionName_ValueApplied(string value)
        {
            var c = new MongoConfiguration()
            {
                StationTagsCollectionName = value,
            };

            c.StationTagsCollectionName.Should().Be(value);
        }

    }
}
