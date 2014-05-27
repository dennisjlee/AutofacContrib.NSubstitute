﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Indexed;
using NSubstitute;

namespace AutofacContrib.NSubstitute
{
    /// <summary> Resolves unknown interfaces and Mocks using the <see cref="Substitute"/>. </summary>
    internal class NSubstituteRegistrationHandler : IRegistrationSource
    {
        /// <summary>
        /// Retrieve a registration for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registrationAccessor"></param>
        /// <returns>
        /// Registrations for the service.
        /// </returns>
        public IEnumerable<IComponentRegistration> RegistrationsFor
            (Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            var keyedService = service as KeyedService;
            if (keyedService != null)
            {
                if (!keyedService.ServiceType.IsInterface ||
                    keyedService.ServiceType.IsGenericType &&
                    keyedService.ServiceType.GetGenericTypeDefinition() == typeof (IEnumerable<>) ||
                    keyedService.ServiceType.IsArray ||
                    typeof (IStartable).IsAssignableFrom(keyedService.ServiceType))
                    return Enumerable.Empty<IComponentRegistration>();

                return new[]
                {
                    RegistrationBuilder.ForDelegate((c, p) => Substitute.For(new[] { keyedService.ServiceType }, null))
                    .As(service)
                    .InstancePerLifetimeScope()
                    .CreateRegistration()
                };
            }

            var typedService = service as TypedService;
            if (typedService == null ||
                !typedService.ServiceType.IsInterface ||
                typedService.ServiceType.IsGenericType && typedService.ServiceType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                typedService.ServiceType.IsArray ||
                typeof(IStartable).IsAssignableFrom(typedService.ServiceType))
                return Enumerable.Empty<IComponentRegistration>();

            var rb = RegistrationBuilder.ForDelegate((c, p) => Substitute.For(new[] { typedService.ServiceType }, null))
                .As(service)
                .InstancePerLifetimeScope();

            return new[] { rb.CreateRegistration() };
        }

        public bool IsAdapterForIndividualComponents
        {
            get { return false; }
        }
    }
}