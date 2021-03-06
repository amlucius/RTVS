﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.R.Platform.OS {
    public sealed class ProcessWaitHandle : WaitHandle {
        public ProcessWaitHandle(SafeProcessHandle processHandle) {
            var currentProcess = NativeMethods.GetCurrentProcess();
            if(!NativeMethods.DuplicateHandle(currentProcess, processHandle, currentProcess, out SafeWaitHandle handle, 0, false, (uint)NativeMethods.DuplicateOptions.DUPLICATE_SAME_ACCESS)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            this.SetSafeWaitHandle(handle);
         }
    }
}
