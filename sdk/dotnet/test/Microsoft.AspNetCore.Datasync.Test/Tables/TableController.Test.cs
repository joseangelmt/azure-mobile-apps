﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.InMemory;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.AspNetCore.Datasync.Test.Tables
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class TableController_Tests
    {
        [Fact]
        public void Repository_Throws_WhenSetNull()
        {
            var controller = new TableController<InMemoryMovie>();
            Assert.Throws<ArgumentNullException>(() => controller.Repository = null);
        }

        [Fact]
        public void Repository_Throws_WhenGetNull()
        {
            var controller = new TableController<InMemoryMovie>();
            Assert.Throws<InvalidOperationException>(() => controller.Repository);
        }

        [Fact]
        [SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Proper split of arrange/act/assert")]
        public void Repository_CanBeStored()
        {
            var repository = new InMemoryRepository<InMemoryMovie>();
            var controller = new TableController<InMemoryMovie>() { Repository = repository };
            Assert.NotNull(controller.Repository);
            Assert.Equal(repository, controller.Repository);
        }
    }
}
