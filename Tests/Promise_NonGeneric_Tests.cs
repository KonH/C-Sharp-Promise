﻿using Moq;
using RSG;
using RSG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace RSG.Tests
{
    public class Promise_NonGeneric_Tests
    {
        [Fact]
        public void can_resolve_simple_promise()
        {
            var promise = Promise.Resolved();

            var completed = 0;
            promise.Then(() => ++completed);

            Assert.Equal(1, completed);
        }

        [Fact]
        public void can_reject_simple_promise()
        {
            var ex = new Exception();
            var promise = Promise.Rejected(ex);

            var errors = 0;
            promise.Catch(e =>
            {
                Assert.Equal(ex, e);
                ++errors;
            });

            Assert.Equal(1, errors);
        }

        [Fact]
        public void exception_is_thrown_for_reject_after_reject()
        {
            var promise = new Promise();

            promise.Reject(new ApplicationException());

            Assert.Throws<ApplicationException>(() =>
                promise.Reject(new ApplicationException())
            );
        }

        [Fact]
        public void exception_is_thrown_for_reject_after_resolve()
        {
            var promise = new Promise();

            promise.Resolve();

            Assert.Throws<ApplicationException>(() =>
                promise.Reject(new ApplicationException())
            );
        }

        [Fact]
        public void exception_is_thrown_for_resolve_after_reject()
        {
            var promise = new Promise();

            promise.Reject(new ApplicationException());

            Assert.Throws<ApplicationException>(() =>
                promise.Resolve()
            );
        }

        [Fact]
        public void can_resolve_promise_and_trigger_then_handler()
        {
            var promise = new Promise();

            var completed = 0;

            promise.Then(() => ++completed);

            promise.Resolve();

            Assert.Equal(1, completed);
        }

        [Fact]
        public void exception_is_thrown_for_resolve_after_resolve()
        {
            var promise = new Promise();

            promise.Resolve();

            Assert.Throws<ApplicationException>(() =>
                promise.Resolve()
            );
        }

        [Fact]
        public void can_resolve_promise_and_trigger_multiple_then_handlers_in_order()
        {
            var promise = new Promise();

            var completed = 0;

            promise.Then(() => Assert.Equal(1, ++completed));
            promise.Then(() => Assert.Equal(2, ++completed));

            promise.Resolve();

            Assert.Equal(2, completed);
        }

        [Fact]
        public void can_resolve_promise_and_trigger_then_handler_with_callback_registration_after_resolve()
        {
            var promise = new Promise();

            var completed = 0;

            promise.Resolve();

            promise.Then(() => ++completed);

            Assert.Equal(1, completed);
        }

        [Fact]
        public void can_reject_promise_and_trigger_error_handler()
        {
            var promise = new Promise();

            var ex = new ApplicationException();
            var completed = 0;
            promise.Catch(e =>
            {
                Assert.Equal(ex, e);
                ++completed;
            });

            promise.Reject(ex);

            Assert.Equal(1, completed);
        }

        [Fact]
        public void can_reject_promise_and_trigger_multiple_error_handlers_in_order()
        {
            var promise = new Promise();

            var ex = new ApplicationException();
            var completed = 0;

            promise.Catch(e =>
            {
                Assert.Equal(ex, e);
                Assert.Equal(1, ++completed);
            });
            promise.Catch(e =>
            {
                Assert.Equal(ex, e);
                Assert.Equal(2, ++completed);
            });

            promise.Reject(ex);

            Assert.Equal(2, completed);
        }

        [Fact]
        public void can_reject_promise_and_trigger_error_handler_with_registration_after_reject()
        {
            var promise = new Promise();

            var ex = new ApplicationException();
            promise.Reject(ex);

            var completed = 0;
            promise.Catch(e =>
            {
                Assert.Equal(ex, e);
                ++completed;
            });

            Assert.Equal(1, completed);
        }

        [Fact]
        public void error_handler_is_not_invoked_for_resolved_promised()
        {
            var promise = new Promise();

            promise.Catch(e =>
            {
                throw new ApplicationException("This shouldn't happen");
            });

            promise.Resolve();
        }

        [Fact]
        public void then_handler_is_not_invoked_for_rejected_promise()
        {
            var promise = new Promise();

            promise.Then(() =>
            {
                throw new ApplicationException("This shouldn't happen");
            });

            promise.Reject(new ApplicationException("Rejection!"));
        }

        [Fact]
        public void chain_multiple_promises_using_all()
        {
            var promise = new Promise();
            var chainedPromise1 = new Promise();
            var chainedPromise2 = new Promise();

            var completed = 0;

            promise
                .ThenAll(() => LinqExts.FromItems(chainedPromise1, chainedPromise2).Cast<IPromise>())
                .Then(() => ++completed);

            Assert.Equal(0, completed);

            promise.Resolve();

            Assert.Equal(0, completed);

            chainedPromise1.Resolve();

            Assert.Equal(0, completed);

            chainedPromise2.Resolve();

            Assert.Equal(1, completed);
        }

        [Fact]
        public void chain_multiple_promises_using_all_that_are_resolved_out_of_order()
        {
            var promise = new Promise();
            var chainedPromise1 = new Promise<int>();
            var chainedPromise2 = new Promise<int>();
            var chainedResult1 = 10;
            var chainedResult2 = 15;

            var completed = 0;

            promise
                .ThenAll(() => LinqExts.FromItems(chainedPromise1, chainedPromise2).Cast<IPromise<int>>())
                .Then(result =>
                {
                    var items = result.ToArray();
                    Assert.Equal(2, items.Length);
                    Assert.Equal(chainedResult1, items[0]);
                    Assert.Equal(chainedResult2, items[1]);

                    ++completed;
                });

            Assert.Equal(0, completed);

            promise.Resolve();

            Assert.Equal(0, completed);

            chainedPromise1.Resolve(chainedResult1);

            Assert.Equal(0, completed);

            chainedPromise2.Resolve(chainedResult2);

            Assert.Equal(1, completed);
        }

        [Fact]
        public void chain_multiple_value_promises_using_all_resolved_out_of_order()
        {
            var promise = new Promise();
            var chainedPromise1 = new Promise<int>();
            var chainedPromise2 = new Promise<int>();
            var chainedResult1 = 10;
            var chainedResult2 = 15;

            var completed = 0;

            promise
                .ThenAll(() => LinqExts.FromItems(chainedPromise1, chainedPromise2).Cast<IPromise<int>>())
                .Then(result =>
                {
                    var items = result.ToArray();
                    Assert.Equal(2, items.Length);
                    Assert.Equal(chainedResult1, items[0]);
                    Assert.Equal(chainedResult2, items[1]);

                    ++completed;
                });

            Assert.Equal(0, completed);

            promise.Resolve();

            Assert.Equal(0, completed);

            chainedPromise2.Resolve(chainedResult2);

            Assert.Equal(0, completed);

            chainedPromise1.Resolve(chainedResult1);

            Assert.Equal(1, completed);
        }

        [Fact]
        public void combined_promise_is_resolved_when_children_are_resolved()
        {
            var promise1 = new Promise();
            var promise2 = new Promise();

            var all = Promise.All(LinqExts.FromItems<IPromise>(promise1, promise2));

            var completed = 0;

            all.Then(() => ++completed);

            promise1.Resolve();
            promise2.Resolve();

            Assert.Equal(1, completed);
        }

        [Fact]
        public void combined_promise_is_rejected_when_first_promise_is_rejected()
        {
            var promise1 = new Promise();
            var promise2 = new Promise();

            var all = Promise.All(LinqExts.FromItems<IPromise>(promise1, promise2));

            all.Then(() =>
            {
                throw new ApplicationException("Shouldn't happen");
            });

            var errors = 0;
            all.Catch(e =>
            {
                ++errors;
            });

            promise1.Reject(new ApplicationException("Error!"));
            promise2.Resolve();

            Assert.Equal(1, errors);
        }

        [Fact]
        public void combined_promise_is_rejected_when_second_promise_is_rejected()
        {
            var promise1 = new Promise();
            var promise2 = new Promise();

            var all = Promise.All(LinqExts.FromItems<IPromise>(promise1, promise2));

            all.Then(() =>
            {
                throw new ApplicationException("Shouldn't happen");
            });

            var errors = 0;
            all.Catch(e =>
            {
                ++errors;
            });

            promise1.Resolve();
            promise2.Reject(new ApplicationException("Error!"));

            Assert.Equal(1, errors);
        }

        [Fact]
        public void combined_promise_is_rejected_when_both_promises_are_rejected()
        {
            var promise1 = new Promise();
            var promise2 = new Promise();

            var all = Promise.All(LinqExts.FromItems<IPromise>(promise1, promise2));

            all.Then(() =>
            {
                throw new ApplicationException("Shouldn't happen");
            });

            var errors = 0;
            all.Catch(e =>
            {
                ++errors;
            });

            promise1.Reject(new ApplicationException("Error!"));
            promise2.Reject(new ApplicationException("Error!"));

            Assert.Equal(1, errors);
        }

        [Fact]
        public void combined_promise_is_resolved_if_there_are_no_promises()
        {
            var all = Promise.All(LinqExts.Empty<IPromise>());

            var completed = 0;

            all.Then(() => ++completed);
        }

        [Fact]
        public void combined_promise_is_resolved_when_all_promises_are_already_resolved()
        {
            var promise1 = Promise.Resolved();
            var promise2 = Promise.Resolved();

            var all = Promise.All(LinqExts.FromItems(promise1, promise2));

            var completed = 0;

            all.Then(() =>
            {
                ++completed;
            });

            Assert.Equal(1, completed);
        }

        [Fact]
        public void exception_thrown_during_transform_rejects_promise()
        {
            var promise = new Promise();

            var errors = 0;
            var ex = new Exception();

            var transformedPromise = promise
                .Then(() => 
                {
                    throw ex;
                })
                .Catch(e =>
                {
                    Assert.Equal(ex, e);

                    ++errors;
                });

            promise.Resolve();

            Assert.Equal(1, errors);
        }

        [Fact]
        public void can_chain_promise()
        {
            var promise = new Promise();
            var chainedPromise = new Promise();

            var completed = 0;

            promise
                .Then(() => chainedPromise)
                .Then(() => ++completed);

            promise.Resolve();
            chainedPromise.Resolve();

            Assert.Equal(1, completed);
        }

        [Fact]
        public void can_chain_promise_and_convert_to_promise_that_yields_a_value()
        {
            var promise = new Promise();
            var chainedPromise = new Promise<string>();
            var chainedPromiseValue = "some-value";

            var completed = 0;

            promise
                .Then(() => chainedPromise)
                .Then(v => 
                {
                    Assert.Equal(chainedPromiseValue, v);

                    ++completed;
                });

            promise.Resolve();
            chainedPromise.Resolve(chainedPromiseValue);

            Assert.Equal(1, completed);
        }

        [Fact]
        public void exception_thrown_in_chain_rejects_resulting_promise()
        {
            var promise = new Promise();
            var chainedPromise = new Promise();

            var ex = new Exception();
            var errors = 0;

            promise
                .Then(() =>
                {
                    throw ex;
                })
                .Catch(e =>
                {
                    Assert.Equal(ex, e);

                    ++errors;
                });

            promise.Resolve();

            Assert.Equal(1, errors);
        }

        [Fact]
        public void rejection_of_source_promise_rejects_chained_promise()
        {
            var promise = new Promise();
            var chainedPromise = new Promise();

            var ex = new Exception();
            var errors = 0;

            promise
                .Then(() => chainedPromise)
                .Catch(e =>
                {
                    Assert.Equal(ex, e);

                    ++errors;
                });

            promise.Reject(ex);

            Assert.Equal(1, errors);
        }

        [Fact]
        public void race_is_resolved_when_first_promise_is_resolved_first()
        {
            var promise1 = new Promise();
            var promise2 = new Promise();

            var completed = 0;

            Promise
                .Race(promise1, promise2)
                .Then(() => ++completed);

            promise1.Resolve();

            Assert.Equal(1, completed);
        }

        [Fact]
        public void race_is_resolved_when_second_promise_is_resolved_first()
        {
            var promise1 = new Promise();
            var promise2 = new Promise();

            var completed = 0;

            Promise
                .Race(promise1, promise2)
                .Then(() => ++completed);

            promise2.Resolve();

            Assert.Equal(1, completed);
        }

        [Fact]
        public void race_is_rejected_when_first_promise_is_rejected_first()
        {
            var promise1 = new Promise();
            var promise2 = new Promise();

            Exception ex = null;

            Promise
                .Race(promise1, promise2)
                .Catch(e => ex = e);

            var expected = new Exception();
            promise1.Reject(expected);

            Assert.Equal(expected, ex);
        }

        [Fact]
        public void race_is_rejected_when_second_promise_is_rejected_first()
        {
            var promise1 = new Promise();
            var promise2 = new Promise();

            Exception ex = null;

            Promise
                .Race(promise1, promise2)
                .Catch(e => ex = e);

            var expected = new Exception();
            promise2.Reject(expected);

            Assert.Equal(expected, ex);
        }

        [Fact]
        public void sequence_with_no_operations_is_directly_resolved()
        {
            var completed = 0;

            Promise
                .Sequence(new Func<IPromise>[0])
                .Then(() => ++completed);

            Assert.Equal(1, completed);
        }

        [Fact]
        public void sequenced_is_not_resolved_when_operation_is_not_resolved()
        {
            var completed = 0;

            Promise
                .Sequence(() => new Promise())
                .Then(() => ++completed);

            Assert.Equal(0, completed);
        }

        [Fact]
        public void sequence_is_resolved_when_operation_is_resolved()
        {
            var completed = 0;

            Promise
                .Sequence(() => Promise.Resolved())
                .Then(() => ++completed);

            Assert.Equal(1, completed);
        }

        [Fact]
        public void sequence_is_unresolved_when_some_operations_are_unresolved()
        {
            var completed = 0;

            Promise
                .Sequence(
                    () => Promise.Resolved(),
                    () => new Promise()
                )
                .Then(() => ++completed);

            Assert.Equal(0, completed);
        }

        [Fact]
        public void sequence_is_resolved_when_all_operations_are_resolved()
        {
            var completed = 0;

            Promise
                .Sequence(
                    () => Promise.Resolved(),
                    () => Promise.Resolved()
                )
                .Then(() => ++completed);

            Assert.Equal(1, completed);
        }

        [Fact]
        public void sequenced_operations_are_run_in_order_is_directly_resolved()
        {
            var order = 0;

            Promise
                .Sequence(
                    () =>
                    {
                        Assert.Equal(1, ++order);
                        return Promise.Resolved();
                    },
                    () =>
                    {
                        Assert.Equal(2, ++order);
                        return Promise.Resolved();
                    },
                    () =>
                    {
                        Assert.Equal(3, ++order);
                        return Promise.Resolved();
                    }
                );

            Assert.Equal(3, order);
        }

        [Fact]
        public void exception_thrown_in_sequence_rejects_the_promise()
        {
            var errored = 0;
            var completed = 0;
            var ex = new Exception();

            Promise
                .Sequence(() => 
                {
                    throw ex;
                })
                .Catch(e => {
                    Assert.Equal(ex, e);
                    ++errored;
                })
                .Then(() => ++completed);

            Assert.Equal(1, errored);
            Assert.Equal(0, completed);
        }

        [Fact]
        public void exception_thrown_in_sequence_stops_following_operations_from_being_invoked()
        {
            var completed = 0;

            Promise
                .Sequence(
                    () => 
                    {
                        ++completed;
                        return Promise.Resolved();
                    },
                    () =>
                    {
                        throw new Exception();
                    },
                    () =>
                    {
                        ++completed;
                        return Promise.Resolved();
                    }
                );

            Assert.Equal(1, completed);
        }

        [Fact]
        public void can_resolve_promise_via_resolver_function()
        {
            var promise = new Promise((resolve, reject) =>
            {
                resolve();
            });

            var completed = 0;
            promise.Then(() =>
            {
                ++completed;
            });

            Assert.Equal(1, completed);
        }

        [Fact]
        public void can_reject_promise_via_reject_function()
        {
            var ex = new Exception();
            var promise = new Promise((resolve, reject) =>
            {
                reject(ex);
            });

            var completed = 0;
            promise.Catch(e =>
            {
                Assert.Equal(ex, e);
                ++completed;
            });

            Assert.Equal(1, completed);
        }

        [Fact]
        public void exception_thrown_during_resolver_rejects_proimse()
        {
            var ex = new Exception();
            var promise = new Promise((resolve, reject) =>
            {
                throw ex;
            });

            var completed = 0;
            promise.Catch(e =>
            {
                Assert.Equal(ex, e);
                ++completed;
            });

            Assert.Equal(1, completed);
        }

        [Fact]
        public void unhandled_exception_is_propagated_via_event()
        {
            var promise = new Promise();
            var ex = new Exception();
            var eventRaised = 0;

            EventHandler<ExceptionEventArgs> handler = (s, e) =>
            {
                Assert.Equal(ex, e.Exception);

                ++eventRaised;
            };

            Promise.UnhandledException += handler;

            try
            {
                promise
                    .Then(() =>
                    {
                        throw ex;
                    })
                    .Done();

                promise.Resolve();

                Assert.Equal(1, eventRaised);
            }
            finally
            {
                Promise.UnhandledException -= handler;
            }
        }

        [Fact]
        public void handled_exception_is_not_propagated_via_event()
        {
            var promise = new Promise();
            var ex = new Exception();
            var eventRaised = 0;

            EventHandler<ExceptionEventArgs> handler = (s, e) => ++eventRaised;

            Promise.UnhandledException += handler;

            try
            {
                promise
                    .Then(() =>
                    {
                        throw ex;
                    })
                    .Catch(_ =>
                    {
                        // Catch the error.
                    })
                    .Done();

                promise.Resolve();

                Assert.Equal(1, eventRaised);
            }
            finally
            {
                Promise.UnhandledException -= handler;
            }

        }

        [Fact]
        public void can_handle_Done_onResolved()
        {
            var promise = new Promise();
            var callback = 0;

            promise.Done(() => ++callback);

            promise.Resolve();

            Assert.Equal(1, callback);
        }

        [Fact]
        public void can_handle_Done_onResolved_with_onReject()
        {
            var promise = new Promise();
            var callback = 0;
            var errorCallback = 0;

            promise.Done(
                () => ++callback,
                ex => ++errorCallback
            );

            promise.Resolve();

            Assert.Equal(1, callback);
            Assert.Equal(0, errorCallback);
        }

        /*todo:
         * Also want a test that exception thrown during Then triggers the error handler.
         * How do Javascript promises work in this regard?
        [Fact]
        public void exception_during_Done_onResolved_triggers_error_hander()
        {
            var promise = new Promise();
            var callback = 0;
            var errorCallback = 0;
            var expectedValue = 5;
            var expectedException = new Exception();

            promise.Done(
                value =>
                {
                    Assert.Equal(expectedValue, value);

                    ++callback;

                    throw expectedException;
                },
                ex =>
                {
                    Assert.Equal(expectedException, ex);

                    ++errorCallback;
                }
            );

            promise.Resolve(expectedValue);

            Assert.Equal(1, callback);
            Assert.Equal(1, errorCallback);
        }
         * */

        [Fact]
        public void exception_during_Then_onResolved_triggers_error_hander()
        {
            var promise = new Promise();
            var callback = 0;
            var errorCallback = 0;
            var expectedException = new Exception();

            promise
                .Then(() =>
                {
                    throw expectedException;

                    return Promise.Resolved();
                })
                .Done(
                    () => ++callback,
                    ex =>
                    {
                        Assert.Equal(expectedException, ex);

                        ++errorCallback;
                    }
                );

            promise.Resolve();

            Assert.Equal(0, callback);
            Assert.Equal(1, errorCallback);
        }
    }
}
