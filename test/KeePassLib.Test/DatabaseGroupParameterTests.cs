using KeePass.Models;
using System;
using Xunit;

namespace KeePass.Tests
{
    public class DatabaseGroupParameterTests
    {
        [Fact]
        public void ParameterEncode()
        {
            var db = new KeePassId(Guid.NewGuid());
            var group = new KeePassId(Guid.NewGuid());

            var parameter = DatabaseGroupParameter.Encode(db, group);

            var d = db.Id.ToString();
            var g = group.Id.ToString();

            Assert.Equal(db.Id.ToString("N") + group.Id.ToString("N"), parameter);
        }

        [Fact]
        public void ParameterDecode()
        {
            var db = Guid.Parse("070a6a32-cb1f-40b8-93f0-8f04cc68c662");
            var group = Guid.Parse("75b4dd9e-2268-4ab6-876a-c05bd917a64d");
            var parameter = DatabaseGroupParameter.Decode("070a6a32cb1f40b893f08f04cc68c66275b4dd9e22684ab6876ac05bd917a64d");

            Assert.Equal(db, parameter.Database.Id);
            Assert.Equal(group, parameter.Group.Id);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(32)]
        [InlineData(63)]
        [InlineData(65)]
        public void InvalidDecode(int length)
        {
            // Must still generate values that would be a valid guid pair
            var data = new string('0', length);
            Assert.Throws<ArgumentOutOfRangeException>("encodedParameter", () => DatabaseGroupParameter.Decode(data));
        }
    }
}
