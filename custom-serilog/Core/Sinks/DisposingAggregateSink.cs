// Copyright 2020 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#nullable enable
using Serilog.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Events;

namespace Serilog.Core.Sinks
{
    class DisposingAggregateSink : ILogEventSink, IDisposable
    {
        readonly ILogEventSink[] _sinks;

        public DisposingAggregateSink(IEnumerable<ILogEventSink> sinks)
        {
            if (sinks == null) throw new ArgumentNullException(nameof(sinks));
            _sinks = sinks.ToArray();
        }

        public void Emit(LogEvent logEvent)
        {
            List<Exception>? exceptions = null;
            foreach (var sink in _sinks)
            {
                try
                {
                    sink.Emit(logEvent);
                }
                catch (Exception ex)
                {
                    SelfLog.WriteLine("Caught exception while emitting to sink {0}: {1}", sink, ex);
                    exceptions ??= new();
                    exceptions.Add(ex);
                }
            }

            if (exceptions != null)
                throw new AggregateException("Failed to emit a log event.", exceptions);
        }

        public void Dispose()
        {
            foreach (var sink in _sinks)
            {
                if (!(sink is IDisposable disposable)) continue;

                try
                {
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    SelfLog.WriteLine("Caught exception while disposing sink {0}: {1}", sink, ex);
                }
            }
        }
    }
}
