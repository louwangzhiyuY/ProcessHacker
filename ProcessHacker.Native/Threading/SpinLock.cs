﻿/*
 * Process Hacker - 
 *   spinlock
 *
 * Copyright (C) 2009 wj32
 * 
 * This file is part of Process Hacker.
 * 
 * Process Hacker is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Process Hacker is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Process Hacker.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ProcessHacker.Native.Threading
{
    /// <summary>
    /// Represents a spinlock, a high-performance mutual exclusion lock.
    /// </summary>
    public class SpinLock
    {
        public class SpinLockContext : IDisposable
        {
            private SpinLock _spinLock;

            internal SpinLockContext(SpinLock spinLock)
            {
                _spinLock = spinLock;
                _spinLock.Acquire();
            }

            public void Dispose()
            {
                _spinLock.Release();
            }
        }

        private int _value = 0;
        private bool _spin;
        private int _spinCount;

        /// <summary>
        /// Creates a spinlock.
        /// </summary>
        public SpinLock()
            : this(10)
        { }

        /// <summary>
        /// Creates a spinlock.
        /// </summary>
        /// <param name="spinCount">
        /// The number of times to spin if waiting to acquire the lock. 
        /// This value is ignored on uniprocessor systems.
        /// </param>
        public SpinLock(int spinCount)
        {
            _spinCount = spinCount;

            // We don't want to spin on uniprocessor systems.
            if (Environment.ProcessorCount == 1)
                _spin = false;
            else
                _spin = true;
        }

        /// <summary>
        /// Gets or sets the spin count.
        /// </summary>
        public int SpinCount
        {
            get { return _spinCount; }
            set { _spinCount = value; }
        }

        /// <summary>
        /// Acquires the spinlock.
        /// </summary>
        public void Acquire()
        {
            if (_spin)
            {
                while (Interlocked.CompareExchange(ref _value, 1, 0) == 1)
                    Thread.SpinWait(_spinCount);
            }
            else
            {
                while (Interlocked.CompareExchange(ref _value, 1, 0) == 1)
                    Thread.Sleep(0);
            }
        }

        /// <summary>
        /// Acquires the spinlock using a context object.
        /// </summary>
        /// <returns>A disposable context object.</returns>
        public SpinLockContext AcquireContext()
        {
            return new SpinLockContext(this);
        }

        /// <summary>
        /// Releases the spinlock.
        /// </summary>
        public void Release()
        {
            Interlocked.Exchange(ref _value, 0);
        }
    }
}
