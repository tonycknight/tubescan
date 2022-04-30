using System;
using FluentAssertions;
using TubeScan.Config;
using Xunit;

namespace TubeScan.Tests.Config
{
    public class AppConfigurationExtensionsTests
    {

        [Fact]
        public void Validate_NullConfig_ThrowsException()
        {
            AppConfiguration? config = null;

#pragma warning disable CS8604 // Possible null reference argument.
            Action f = () => config.Validate();
#pragma warning restore CS8604 // Possible null reference argument.

            f.Should().Throw<InvalidOperationException>().WithMessage("?*");
        }

        [Fact]
        public void Validate_NullDiscordConfig_ThrowsException()
        {
            var config = new AppConfiguration()
            {
                Discord = null,
            };

            Action f = () => config.Validate();

            f.Should().Throw<InvalidOperationException>().WithMessage("?*");
        }

        [Fact]
        public void Validate_NullMongoConfig_ThrowsException()
        {
            var config = new AppConfiguration()
            {
                Mongo = null,
            };

            Action f = () => config.Validate();

            f.Should().Throw<InvalidOperationException>().WithMessage("?*");
        }

        [Theory]
        [InlineData(null, null, null, null, null)]
        [InlineData("", "a", "b", "c", "e")]
        [InlineData(" ", "a", "b", "c", "e")]
        [InlineData("a", "", "a", "b", "e")]
        [InlineData("a", " ", "c", "d", "e")]
        [InlineData("a", "b", "", "d", "e")]
        [InlineData("a", "b", " ", "d", "e")]
        [InlineData("a", "b", "c", "", "e")]
        [InlineData("a", "b", "c", " ", "e")]
        [InlineData("a", "b", "c", "d", "")]
        [InlineData("a", "b", "c", "d", " ")]
        [InlineData("", "", "", "", "")]
        public void Validate_InvalidValues_ThrowsException(string clientId, string clientToken, string connection, string db, string appKey)
        {
            var config = new AppConfiguration()
            {
                Discord = new DiscordConfiguration()
                {
                    ClientId = clientId,
                    ClientToken = clientToken,
                },
                Mongo = new MongoConfiguration()
                {
                    Connection = connection,
                    DatabaseName = db,
                },
                Tfl = new TflConfiguration()
                {
                    AppKey = appKey
                }
            };


            Action f = () => config.Validate();

            f.Should().Throw<InvalidOperationException>().WithMessage("?*");
        }

        [Theory]
        [InlineData("a", "b", "c", "d", "e")]
        [InlineData("1", "2", "3", "4", "5")]
        public void Validate_ValidValues_ReturnsConfig(string clientId, string clientToken, string connection, string db, string appKey)
        {
            var config = new AppConfiguration()
            {
                Discord = new DiscordConfiguration()
                {
                    ClientId = clientId,
                    ClientToken = clientToken,
                },
                Mongo = new MongoConfiguration()
                {
                    Connection = connection,
                    DatabaseName = db,
                },
                Tfl = new TflConfiguration()
                {
                    AppKey = appKey
                }
            };


            var r = config.Validate();

            r.Should().Be(config);
        }
    }
}
