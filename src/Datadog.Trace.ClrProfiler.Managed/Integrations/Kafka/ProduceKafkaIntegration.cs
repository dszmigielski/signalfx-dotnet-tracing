// Modified by SignalFx

using System;
using Datadog.Trace.ClrProfiler.Emit;
using SignalFx.Tracing;
using SignalFx.Tracing.Logging;

namespace Datadog.Trace.ClrProfiler.Integrations.Kafka
{
    /// <summary>
    /// Tracer integration for Kafka Produce method.
    /// </summary>
    public static class ProduceKafkaIntegration
    {
        private const string ProduceSyncOperationName = "kafka.produce";

        private static readonly SignalFx.Tracing.Vendors.Serilog.ILogger Log = SignalFxLogging.GetLogger(typeof(ProduceKafkaIntegration));

        /// <summary>
        /// Traces a synchronous Produce call to Kafka.
        /// </summary>
        /// <param name="producer">The producer for the original method.</param>
        /// <param name="topic">The topic to produce the message to.</param>
        /// <param name="message">The message to produce.</param>
        /// <param name="deliveryHandler">A delegate that will be called with a delivery report corresponding to the produce request (if enabled).</param>
        /// <param name="opCode">The OpCode used in the original method call.</param>
        /// <param name="mdToken">The mdToken of the original method call.</param>
        /// <param name="moduleVersionPtr">A pointer to the module version GUID.</param>
        /// <returns>The original result</returns>
        [InterceptMethod(
            TargetAssembly = Constants.ConfluentKafkaAssemblyName,
            TargetType = Constants.ProducerType,
            TargetMethod = Constants.ProduceSyncMethodName,
            TargetSignatureTypes = new[] { ClrNames.Void, Constants.TopicPartitionTypeName, Constants.MessageTypeName, Constants.ActionOfDeliveryReportTypeName },
            TargetMinimumVersion = Constants.MinimumVersion,
            TargetMaximumVersion = Constants.MaximumVersion)]
        public static object ProduceWithTopicPartitionTopic(
            object producer,
            object topic,
            object message,
            object deliveryHandler,
            int opCode,
            int mdToken,
            long moduleVersionPtr)
        {
            if (producer is null)
            {
                throw new ArgumentNullException(nameof(producer));
            }

            var scope = KafkaHelper.CreateProduceScope(topic, message, ProduceSyncOperationName);

            var headers = KafkaHelper.GetPropertyValue<object>(message, "Headers") ?? KafkaHelper.CreateHeaders(message);
            if (headers is not null)
            {
                var headerAdapter = new KafkaHeadersCollectionAdapter(headers);
                Tracer.Instance.Propagator
                    .Inject(scope.Span.Context, headerAdapter, (collectionAdapter, key, value) => collectionAdapter.Set(key, value));
            }

            const string methodName = Constants.ProduceSyncMethodName;
            Action<object, object, object, object> produce;
            var producerType = producer.GetType();

            try
            {
                produce =
                    MethodBuilder<Action<object, object, object, object>>
                       .Start(moduleVersionPtr, mdToken, opCode, methodName)
                       .WithConcreteType(producerType)
                       .WithParameters(topic, message, deliveryHandler)
                       .WithNamespaceAndNameFilters(ClrNames.Void, Constants.TopicPartitionTypeName, Constants.MessageTypeName, Constants.ActionOfDeliveryReportTypeName)
                       .Build();
            }
            catch (Exception ex)
            {
                // profiled app will not continue working as expected without this method
                Log.ErrorRetrievingMethod(
                    exception: ex,
                    moduleVersionPointer: moduleVersionPtr,
                    mdToken: mdToken,
                    opCode: opCode,
                    instrumentedType: Constants.ProducerType,
                    methodName: methodName,
                    instanceType: producer.GetType().AssemblyQualifiedName);
                throw;
            }

            try
            {
                produce(producer, topic, message, deliveryHandler);
                return null;
            }
            catch (Exception ex) when (scope.Span.SetExceptionForFilter(ex))
            {
                throw;
            }
            finally
            {
                scope.Dispose();
            }
        }

        /// <summary>
        /// Traces a synchronous Produce call to Kafka.
        /// </summary>
        /// <param name="producer">The producer for the original method.</param>
        /// <param name="topic">The topic to produce the message to.</param>
        /// <param name="message">The message to produce.</param>
        /// <param name="deliveryHandler">A delegate that will be called with a delivery report corresponding to the produce request (if enabled).</param>
        /// <param name="opCode">The OpCode used in the original method call.</param>
        /// <param name="mdToken">The mdToken of the original method call.</param>
        /// <param name="moduleVersionPtr">A pointer to the module version GUID.</param>
        /// <returns>The original result</returns>
        [InterceptMethod(
            TargetAssembly = Constants.ConfluentKafkaAssemblyName,
            TargetType = Constants.ProducerType,
            TargetMethod = Constants.ProduceSyncMethodName,
            TargetSignatureTypes = new[] { ClrNames.Void, ClrNames.String, Constants.MessageTypeName, Constants.ActionOfDeliveryReportTypeName },
            TargetMinimumVersion = Constants.MinimumVersion,
            TargetMaximumVersion = Constants.MaximumVersion)]
        public static object ProduceWithStringTopic(
            object producer,
            object topic,
            object message,
            object deliveryHandler,
            int opCode,
            int mdToken,
            long moduleVersionPtr)
        {
            if (producer is null)
            {
                throw new ArgumentNullException(nameof(producer));
            }

            var scope = KafkaHelper.CreateProduceScope(topic, message, ProduceSyncOperationName);

            var headers = KafkaHelper.GetPropertyValue<object>(message, "Headers") ?? KafkaHelper.CreateHeaders(message);
            if (headers is not null)
            {
                var headerAdapter = new KafkaHeadersCollectionAdapter(headers);
                Tracer.Instance.Propagator
                    .Inject(scope.Span.Context, headerAdapter, (collectionAdapter, key, value) => collectionAdapter.Set(key, value));
            }

            const string methodName = Constants.ProduceSyncMethodName;
            Action<object, object, object, object> produce;
            var producerType = producer.GetType();

            try
            {
                produce =
                    MethodBuilder<Action<object, object, object, object>>
                       .Start(moduleVersionPtr, mdToken, opCode, methodName)
                       .WithConcreteType(producerType)
                       .WithParameters(topic, message, deliveryHandler)
                       .WithNamespaceAndNameFilters(ClrNames.Void, ClrNames.String, Constants.MessageTypeName, Constants.ActionOfDeliveryReportTypeName)
                       .Build();
            }
            catch (Exception ex)
            {
                // profiled app will not continue working as expected without this method
                Log.ErrorRetrievingMethod(
                    exception: ex,
                    moduleVersionPointer: moduleVersionPtr,
                    mdToken: mdToken,
                    opCode: opCode,
                    instrumentedType: Constants.ProducerType,
                    methodName: methodName,
                    instanceType: producer.GetType().AssemblyQualifiedName);
                throw;
            }

            try
            {
                produce(producer, topic, message, deliveryHandler);
                return null;
            }
            catch (Exception ex) when (scope.Span.SetExceptionForFilter(ex))
            {
                throw;
            }
            finally
            {
                scope.Dispose();
            }
        }
    }
}
