// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using LINQPad;
using Metalama.Framework.Code;
using Metalama.Testing.UnitTesting;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Xunit;

namespace Metalama.LinqPad.Tests
{
    public sealed class PropertyFormatterTests : UnitTestClass
    {
        [Fact]
        public void GroupingTest()
        {
            var data = new[] { (1, 2), (1, 3) };
            var groupings = data.GroupBy( d => d.Item1 );
            var grouping = groupings.First();
            var facade = FacadePropertyFormatter.FormatPropertyValueTestable( grouping );
            Assert.IsType<DumpContainer>( facade.View );
            Assert.IsType<GroupingFacade<int, (int, int)>>( facade.ViewModel );
        }

        [Fact]
        public void EnumTest()
        {
            var facade = FacadePropertyFormatter.FormatPropertyValueTestable( Framework.Code.Accessibility.Internal );
            Assert.IsType<string>( facade.View );
        }
    }
}