﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Android.OS;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client.Platforms
{
    /// <summary>
    /// Android-specific implementation of the <see cref="IPlatformInformation"/> interface.
    /// </summary>
    internal class PlatformInformation : IPlatformInformation
    {
        /// <summary>
        /// Information about the OS.
        /// </summary>
        public IOSInformation OS => new OSInformation
        {
            Architecture = System.Environment.OSVersion.Platform.ToString(),
            Name = "Android",
            Version = Build.VERSION.Release
        };

        /// <summary>
        /// True if this is running on an emulated device
        /// </summary>
        public bool IsEmulator => Build.Brand.Equals("generic", StringComparison.InvariantCultureIgnoreCase);

        /// <summary>
        /// Converts the IsEmulator into a string.
        /// </summary>
        /// <remarks>
        /// Excluded from code coverage because the string.Empty version will never be returned in a test situation.
        /// </remarks>
        [ExcludeFromCodeCoverage]
        private string Emulator => IsEmulator ? ";test" : "";

        /// <summary>
        /// The details section of the <c>User-Agent</c> header.
        /// </summary>
        public string UserAgentDetails
        {
            get => $"lang=Managed;os={OS.Name}/{OS.Version};arch={OS.Architecture};version={Platform.AssemblyVersion}{Emulator}";
        }
    }
}
