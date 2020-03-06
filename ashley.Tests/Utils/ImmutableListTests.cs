using System.Collections.Generic;
using ashley.Utils;
using Xunit;

namespace ashley.Tests.Utils
{
    public class ImmutableListTests
    {
        [Fact]
        public void SameValues()
        {
            var list = new List<int>();
            var immutable = new ImmutableList<int>(list);

            Assert.Equal(list.Count, immutable.Count);

            for (var i = 0; i < 10; i++)
            {
                list.Add(i);
            }
            
            Assert.Equal(list.Count, immutable.Count);

            for (var i = 0; i < list.Count; i++)
            {
                Assert.Equal(list[i], immutable[i]);
            }
        }

        [Fact]
        public void Iteration()
        {
            var list = new List<int>();
            var immutable = new ImmutableList<int>(list);

            for (var i = 0; i < 10; i++)
            {
                list.Add(i);
            }

            var expected = 0;
            foreach (var value in immutable)
            {
                Assert.Equal(expected++, value);
            }
        }
    }
}