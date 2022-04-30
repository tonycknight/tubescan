using System;
using System.IO;
using FluentAssertions;
using NSubstitute;
using TubeScan.Io;
using TubeScan.Config;
using Xunit;

namespace TubeScan.Tests.Config
{
    public class FileAppConfigurationProviderTests
    {
        [Fact]
        public void GetAppConfiguration_NullFilePath_ExceptionThrown()
        {
            var io = Substitute.For<IoProvider>();
            var cp = new FileAppConfigurationProvider(io);

            var f = () => cp.GetAppConfiguration();

            f.Should().Throw<InvalidOperationException>().WithMessage("?*");
        }

        [Theory]
        [InlineData("test_token")]
        public void GetAppConfiguration_ConfigReturned(string token)
        {
            var json = $"{{ discord: {{  clientId: \"{token}\" }} }}";
            var jsonbuff = System.Text.Encoding.UTF8.GetBytes(json);
            using var s = new MemoryStream(jsonbuff);
            using var srdr = new StreamReader(s);

            var io = Substitute.For<IIoProvider>();
            io.OpenFileReader(Arg.Any<string>()).Returns(srdr);


            var provider = new FileAppConfigurationProvider(io);
            provider.SetFilePath("dummyfilepath");

            var config = provider.GetAppConfiguration();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            config.Discord.ClientId.Should().Be(token);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }



        [Theory]
        [InlineData(" a ")]
        [InlineData("test_token")]
        [InlineData("test_token\tvalue")]
        public void GetAppConfiguration_PlainText_ExceptionThrown(string token)
        {
            var jsonbuff = System.Text.Encoding.UTF8.GetBytes(token);
            using var s = new MemoryStream(jsonbuff);
            using var srdr = new StreamReader(s);

            var io = Substitute.For<IIoProvider>();
            io.OpenFileReader(Arg.Any<string>()).Returns(srdr);


            var provider = new FileAppConfigurationProvider(io);
            provider.SetFilePath("dummyfilepath");

            Action a = () => provider.GetAppConfiguration();
            a.Should().Throw<InvalidOperationException>().WithMessage("?*");
        }


        [Theory]
        [InlineData(" [] ")]
        public void GetAppConfiguration_InvalidJson_ExceptionThrown(string token)
        {
            var jsonbuff = System.Text.Encoding.UTF8.GetBytes(token);
            using var s = new MemoryStream(jsonbuff);
            using var srdr = new StreamReader(s);

            var io = Substitute.For<IIoProvider>();
            io.OpenFileReader(Arg.Any<string>()).Returns(srdr);


            var provider = new FileAppConfigurationProvider(io);
            provider.SetFilePath("dummyfilepath");

            Action a = () => provider.GetAppConfiguration();
            a.Should().Throw<InvalidOperationException>().WithMessage("?*");
        }
    }
}
