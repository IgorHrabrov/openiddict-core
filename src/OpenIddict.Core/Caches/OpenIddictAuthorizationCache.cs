﻿/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OpenIddict.Abstractions;

namespace OpenIddict.Core
{
    /// <summary>
    /// Provides methods allowing to cache authorizations after retrieving them from the store.
    /// </summary>
    /// <typeparam name="TAuthorization">The type of the Authorization entity.</typeparam>
    public class OpenIddictAuthorizationCache<TAuthorization> : IOpenIddictAuthorizationCache<TAuthorization>, IDisposable where TAuthorization : class
    {
        private readonly MemoryCache _cache;
        private readonly ConcurrentDictionary<string, Lazy<CancellationTokenSource>> _signals;
        private readonly IOpenIddictAuthorizationStore<TAuthorization> _store;

        public OpenIddictAuthorizationCache(
            [NotNull] IOptionsMonitor<OpenIddictCoreOptions> options,
            [NotNull] IOpenIddictAuthorizationStoreResolver resolver)
        {
            _cache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = options.CurrentValue.EntityCacheLimit
            });

            _signals = new ConcurrentDictionary<string, Lazy<CancellationTokenSource>>(StringComparer.Ordinal);
            _store = resolver.Get<TAuthorization>();
        }

        /// <summary>
        /// Add the specified authorization to the cache.
        /// </summary>
        /// <param name="authorization">The authorization to add to the cache.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public async Task AddAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            _cache.Remove(new
            {
                Method = nameof(FindAsync),
                Subject = await _store.GetSubjectAsync(authorization, cancellationToken),
                Client = await _store.GetApplicationIdAsync(authorization, cancellationToken)
            });

            _cache.Remove(new
            {
                Method = nameof(FindAsync),
                Subject = await _store.GetSubjectAsync(authorization, cancellationToken),
                Client = await _store.GetApplicationIdAsync(authorization, cancellationToken),
                Status = await _store.GetStatusAsync(authorization, cancellationToken)
            });

            _cache.Remove(new
            {
                Method = nameof(FindAsync),
                Subject = await _store.GetSubjectAsync(authorization, cancellationToken),
                Client = await _store.GetApplicationIdAsync(authorization, cancellationToken),
                Status = await _store.GetStatusAsync(authorization, cancellationToken),
                Type = await _store.GetTypeAsync(authorization, cancellationToken)
            });

            _cache.Remove(new
            {
                Method = nameof(FindByApplicationIdAsync),
                Identifier = await _store.GetApplicationIdAsync(authorization, cancellationToken)
            });

            _cache.Remove(new
            {
                Method = nameof(FindByIdAsync),
                Identifier = await _store.GetIdAsync(authorization, cancellationToken)
            });

            _cache.Remove(new
            {
                Method = nameof(FindBySubjectAsync),
                Subject = await _store.GetSubjectAsync(authorization, cancellationToken)
            });

            var signal = await CreateExpirationTokenAsync(authorization, cancellationToken);
            if (signal == null)
            {
                throw new InvalidOperationException("An error occurred while creating an expiration signal.");
            }

            using (var entry = _cache.CreateEntry(new
            {
                Method = nameof(FindByIdAsync),
                Identifier = await _store.GetIdAsync(authorization, cancellationToken)
            }))
            {
                entry.AddExpirationToken(signal)
                     .SetSize(1L)
                     .SetValue(authorization);
            }
        }

        /// <summary>
        /// Disposes the resources held by this instance.
        /// </summary>
        public void Dispose()
        {
            foreach (var signal in _signals)
            {
                signal.Value.Value.Dispose();
            }

            _cache.Dispose();
        }

        /// <summary>
        /// Retrieves the authorizations corresponding to the specified
        /// subject and associated with the application identifier.
        /// </summary>
        /// <param name="subject">The subject associated with the authorization.</param>
        /// <param name="client">The client associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorizations corresponding to the subject/client.
        /// </returns>
        public ValueTask<ImmutableArray<TAuthorization>> FindAsync(
            [NotNull] string subject, [NotNull] string client, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            if (string.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client identifier cannot be null or empty.", nameof(client));
            }

            var parameters = new
            {
                Method = nameof(FindAsync),
                Subject = subject,
                Client = client
            };

            if (_cache.TryGetValue(parameters, out ImmutableArray<TAuthorization> authorizations))
            {
                return new ValueTask<ImmutableArray<TAuthorization>>(authorizations);
            }

            async Task<ImmutableArray<TAuthorization>> ExecuteAsync()
            {
                foreach (var authorization in (authorizations = await _store.FindAsync(subject, client, cancellationToken)))
                {
                    await AddAsync(authorization, cancellationToken);
                }

                using (var entry = _cache.CreateEntry(parameters))
                {
                    foreach (var authorization in authorizations)
                    {
                        var signal = await CreateExpirationTokenAsync(authorization, cancellationToken);
                        if (signal == null)
                        {
                            throw new InvalidOperationException("An error occurred while creating an expiration signal.");
                        }

                        entry.AddExpirationToken(signal);
                    }

                    entry.SetSize(authorizations.Length);
                    entry.SetValue(authorizations);
                }

                return authorizations;
            }

            return new ValueTask<ImmutableArray<TAuthorization>>(ExecuteAsync());
        }

        /// <summary>
        /// Retrieves the authorizations matching the specified parameters.
        /// </summary>
        /// <param name="subject">The subject associated with the authorization.</param>
        /// <param name="client">The client associated with the authorization.</param>
        /// <param name="status">The authorization status.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorizations corresponding to the criteria.
        /// </returns>
        public ValueTask<ImmutableArray<TAuthorization>> FindAsync(
            [NotNull] string subject, [NotNull] string client,
            [NotNull] string status, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            if (string.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client identifier cannot be null or empty.", nameof(client));
            }

            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentException("The status cannot be null or empty.", nameof(status));
            }

            var parameters = new
            {
                Method = nameof(FindAsync),
                Subject = subject,
                Client = client,
                Status = status
            };

            if (_cache.TryGetValue(parameters, out ImmutableArray<TAuthorization> authorizations))
            {
                return new ValueTask<ImmutableArray<TAuthorization>>(authorizations);
            }

            async Task<ImmutableArray<TAuthorization>> ExecuteAsync()
            {
                foreach (var authorization in (authorizations = await _store.FindAsync(subject, client, status, cancellationToken)))
                {
                    await AddAsync(authorization, cancellationToken);
                }

                using (var entry = _cache.CreateEntry(parameters))
                {
                    foreach (var authorization in authorizations)
                    {
                        var signal = await CreateExpirationTokenAsync(authorization, cancellationToken);
                        if (signal == null)
                        {
                            throw new InvalidOperationException("An error occurred while creating an expiration signal.");
                        }

                        entry.AddExpirationToken(signal);
                    }

                    entry.SetSize(authorizations.Length);
                    entry.SetValue(authorizations);
                }

                return authorizations;
            }

            return new ValueTask<ImmutableArray<TAuthorization>>(ExecuteAsync());
        }

        /// <summary>
        /// Retrieves the authorizations matching the specified parameters.
        /// </summary>
        /// <param name="subject">The subject associated with the authorization.</param>
        /// <param name="client">The client associated with the authorization.</param>
        /// <param name="status">The authorization status.</param>
        /// <param name="type">The authorization type.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorizations corresponding to the criteria.
        /// </returns>
        public ValueTask<ImmutableArray<TAuthorization>> FindAsync(
            [NotNull] string subject, [NotNull] string client,
            [NotNull] string status, [NotNull] string type, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            if (string.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client identifier cannot be null or empty.", nameof(client));
            }

            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentException("The status cannot be null or empty.", nameof(status));
            }

            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException("The type cannot be null or empty.", nameof(type));
            }

            var parameters = new
            {
                Method = nameof(FindAsync),
                Subject = subject,
                Client = client,
                Status = status,
                Type = type
            };

            if (_cache.TryGetValue(parameters, out ImmutableArray<TAuthorization> authorizations))
            {
                return new ValueTask<ImmutableArray<TAuthorization>>(authorizations);
            }

            async Task<ImmutableArray<TAuthorization>> ExecuteAsync()
            {
                foreach (var authorization in (authorizations = await _store.FindAsync(subject, client, status, type, cancellationToken)))
                {
                    await AddAsync(authorization, cancellationToken);
                }

                using (var entry = _cache.CreateEntry(parameters))
                {
                    foreach (var authorization in authorizations)
                    {
                        var signal = await CreateExpirationTokenAsync(authorization, cancellationToken);
                        if (signal == null)
                        {
                            throw new InvalidOperationException("An error occurred while creating an expiration signal.");
                        }

                        entry.AddExpirationToken(signal);
                    }

                    entry.SetSize(authorizations.Length);
                    entry.SetValue(authorizations);
                }

                return authorizations;
            }

            return new ValueTask<ImmutableArray<TAuthorization>>(ExecuteAsync());
        }

        /// <summary>
        /// Retrieves the authorizations matching the specified parameters.
        /// </summary>
        /// <param name="subject">The subject associated with the authorization.</param>
        /// <param name="client">The client associated with the authorization.</param>
        /// <param name="status">The authorization status.</param>
        /// <param name="type">The authorization type.</param>
        /// <param name="scopes">The minimal scopes associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorizations corresponding to the criteria.
        /// </returns>
        public ValueTask<ImmutableArray<TAuthorization>> FindAsync(
            [NotNull] string subject, [NotNull] string client,
            [NotNull] string status, [NotNull] string type,
            ImmutableArray<string> scopes, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            if (string.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client identifier cannot be null or empty.", nameof(client));
            }

            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentException("The status cannot be null or empty.", nameof(status));
            }

            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException("The type cannot be null or empty.", nameof(type));
            }

            // Note: this method is only partially cached.

            async Task<ImmutableArray<TAuthorization>> ExecuteAsync()
            {
                var authorizations = await _store.FindAsync(subject, client, status, type, scopes, cancellationToken);

                foreach (var authorization in authorizations)
                {
                    await AddAsync(authorization, cancellationToken);
                }

                return authorizations;
            }

            return new ValueTask<ImmutableArray<TAuthorization>>(ExecuteAsync());
        }

        /// <summary>
        /// Retrieves the list of authorizations corresponding to the specified application identifier.
        /// </summary>
        /// <param name="identifier">The application identifier associated with the authorizations.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorizations corresponding to the specified application.
        /// </returns>
        public ValueTask<ImmutableArray<TAuthorization>> FindByApplicationIdAsync(
            [NotNull] string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            var parameters = new
            {
                Method = nameof(FindByApplicationIdAsync),
                Identifier = identifier
            };

            if (_cache.TryGetValue(parameters, out ImmutableArray<TAuthorization> authorizations))
            {
                return new ValueTask<ImmutableArray<TAuthorization>>(authorizations);
            }

            async Task<ImmutableArray<TAuthorization>> ExecuteAsync()
            {
                foreach (var authorization in (authorizations = await _store.FindByApplicationIdAsync(identifier, cancellationToken)))
                {
                    await AddAsync(authorization, cancellationToken);
                }

                using (var entry = _cache.CreateEntry(parameters))
                {
                    foreach (var authorization in authorizations)
                    {
                        var signal = await CreateExpirationTokenAsync(authorization, cancellationToken);
                        if (signal == null)
                        {
                            throw new InvalidOperationException("An error occurred while creating an expiration signal.");
                        }

                        entry.AddExpirationToken(signal);
                    }

                    entry.SetSize(authorizations.Length);
                    entry.SetValue(authorizations);
                }

                return authorizations;
            }

            return new ValueTask<ImmutableArray<TAuthorization>>(ExecuteAsync());
        }

        /// <summary>
        /// Retrieves an authorization using its unique identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorization corresponding to the identifier.
        /// </returns>
        public ValueTask<TAuthorization> FindByIdAsync([NotNull] string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            var parameters = new
            {
                Method = nameof(FindByIdAsync),
                Identifier = identifier
            };

            if (_cache.TryGetValue(parameters, out TAuthorization authorization))
            {
                return new ValueTask<TAuthorization>(authorization);
            }

            async Task<TAuthorization> ExecuteAsync()
            {
                if ((authorization = await _store.FindByIdAsync(identifier, cancellationToken)) != null)
                {
                    await AddAsync(authorization, cancellationToken);
                }

                using (var entry = _cache.CreateEntry(parameters))
                {
                    if (authorization != null)
                    {
                        var signal = await CreateExpirationTokenAsync(authorization, cancellationToken);
                        if (signal == null)
                        {
                            throw new InvalidOperationException("An error occurred while creating an expiration signal.");
                        }

                        entry.AddExpirationToken(signal);
                    }

                    entry.SetSize(1L);
                    entry.SetValue(authorization);
                }

                return authorization;
            }

            return new ValueTask<TAuthorization>(ExecuteAsync());
        }

        /// <summary>
        /// Retrieves all the authorizations corresponding to the specified subject.
        /// </summary>
        /// <param name="subject">The subject associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorizations corresponding to the specified subject.
        /// </returns>
        public ValueTask<ImmutableArray<TAuthorization>> FindBySubjectAsync(
            [NotNull] string subject, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            var parameters = new
            {
                Method = nameof(FindBySubjectAsync),
                Subject = subject
            };

            if (_cache.TryGetValue(parameters, out ImmutableArray<TAuthorization> authorizations))
            {
                return new ValueTask<ImmutableArray<TAuthorization>>(authorizations);
            }

            async Task<ImmutableArray<TAuthorization>> ExecuteAsync()
            {
                foreach (var authorization in (authorizations = await _store.FindBySubjectAsync(subject, cancellationToken)))
                {
                    await AddAsync(authorization, cancellationToken);
                }

                using (var entry = _cache.CreateEntry(parameters))
                {
                    foreach (var authorization in authorizations)
                    {
                        var signal = await CreateExpirationTokenAsync(authorization, cancellationToken);
                        if (signal == null)
                        {
                            throw new InvalidOperationException("An error occurred while creating an expiration signal.");
                        }

                        entry.AddExpirationToken(signal);
                    }

                    entry.SetSize(authorizations.Length);
                    entry.SetValue(authorizations);
                }

                return authorizations;
            }

            return new ValueTask<ImmutableArray<TAuthorization>>(ExecuteAsync());
        }

        /// <summary>
        /// Removes the specified authorization from the cache.
        /// </summary>
        /// <param name="authorization">The authorization to remove from the cache.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public async Task RemoveAsync([NotNull] TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            var identifier = await _store.GetIdAsync(authorization, cancellationToken);
            if (string.IsNullOrEmpty(identifier))
            {
                throw new InvalidOperationException("The application identifier cannot be extracted.");
            }

            if (_signals.TryGetValue(identifier, out Lazy<CancellationTokenSource> signal))
            {
                signal.Value.Cancel();

                _signals.TryRemove(identifier, out signal);
            }
        }

        /// <summary>
        /// Creates an expiration signal allowing to invalidate all the
        /// cache entries associated with the specified authorization.
        /// </summary>
        /// <param name="authorization">The authorization associated with the expiration signal.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns an expiration signal for the specified authorization.
        /// </returns>
        protected virtual async Task<IChangeToken> CreateExpirationTokenAsync(
            [NotNull] TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            var identifier = await _store.GetIdAsync(authorization, cancellationToken);
            if (string.IsNullOrEmpty(identifier))
            {
                throw new InvalidOperationException("The authorization identifier cannot be extracted.");
            }

            var signal = _signals.GetOrAdd(identifier, delegate
            {
                // Note: a Lazy<CancellationTokenSource> is used here to ensure only one CancellationTokenSource
                // can be created. Not doing so would result in expiration signals being potentially linked to
                // multiple sources, with a single one of them being eventually tracked and thus, cancelable.
                return new Lazy<CancellationTokenSource>(() => new CancellationTokenSource());
            });

            return new CancellationChangeToken(signal.Value.Token);
        }
    }
}
