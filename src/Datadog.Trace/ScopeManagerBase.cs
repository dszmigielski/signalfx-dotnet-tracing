// Modified by SignalFx
using System;
using SignalFx.Tracing.Logging;

namespace SignalFx.Tracing
{
    internal abstract class ScopeManagerBase : IScopeManager
    {
        private static readonly SignalFx.Tracing.Vendors.Serilog.ILogger Log = SignalFxLogging.GetLogger(typeof(ScopeManagerBase));

        public event EventHandler<SpanEventArgs> SpanOpened;

        public event EventHandler<SpanEventArgs> SpanActivated;

        public event EventHandler<SpanEventArgs> SpanDeactivated;

        public event EventHandler<SpanEventArgs> SpanClosed;

        public event EventHandler<SpanEventArgs> TraceEnded;

        public abstract Scope Active { get; protected set; }

        public Scope Activate(Span span, bool finishOnClose)
        {
            var newParent = Active;
            var scope = new Scope(newParent, span, this, finishOnClose);
            var scopeOpenedArgs = new SpanEventArgs(span);

            SpanOpened?.Invoke(this, scopeOpenedArgs);

            Active = scope;

            if (newParent != null)
            {
                SpanDeactivated?.Invoke(this, new SpanEventArgs(newParent.Span));
            }

            SpanActivated?.Invoke(this, scopeOpenedArgs);

            return scope;
        }

        public void Close(Scope scope)
        {
            var current = Active;
            var isRootSpan = scope.Parent == null;

            if (current == null || current != scope)
            {
                // This is not the current scope for this context, bail out
                SpanClosed?.Invoke(this, new SpanEventArgs(scope.Span));
                return;
            }

            // if the scope that was just closed was the active scope,
            // set its parent as the new active scope
            Active = scope.Parent;
            SpanDeactivated?.Invoke(this, new SpanEventArgs(scope.Span));

            if (!isRootSpan)
            {
                SpanActivated?.Invoke(this, new SpanEventArgs(scope.Parent.Span));
            }

            SpanClosed?.Invoke(this, new SpanEventArgs(scope.Span));

            if (isRootSpan)
            {
                TraceEnded?.Invoke(this, new SpanEventArgs(scope.Span));
            }
        }
    }
}
