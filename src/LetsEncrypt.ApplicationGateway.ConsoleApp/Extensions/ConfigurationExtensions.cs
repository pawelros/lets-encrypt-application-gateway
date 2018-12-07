﻿namespace LetsEncrypt.ApplicationGateway
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;
    using Serilog;
    public static class ConfigurationExtensions
    {
        /*  NOTE (Cameron): This is a little messy but basically the idea here is that the secret values come
            - first from secrets.json (typically on a developers machine); then
            - second from an environment variable (typically inside of a docker environment).
            If a value is not supplied and there is no default specified (below) then an exception will be thrown.  */

        private static readonly List<(string, string, string)> EnvironmentSecretConfig = new List<(string, string, string)>
        {
            /* secrets.json Key     // Environment Variable     // default value (optional) */
            ("Azure-Client-Id",            "CLIENT_ID",              null),
            ("Azure-Client-Secret",        "AZURE_CLIENT_SECRET",    null),
        };

        private static List<Action<ILogger>> logMessages = new List<Action<ILogger>>();

        public static IConfigurationBuilder AddEnvironmentSecrets(this IConfigurationBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables()
                .Build();

            var secrets = new Dictionary<string, string>();

            foreach (var (key, environmentVariable, defaultValue) in EnvironmentSecretConfig)
            {
                secrets[key] = configuration[key];
                if (secrets[key] != null)
                {
                    logMessages.Add(log => log.Debug("Value of {secretKey} satisfied by secrets.json", key));
                    continue;
                }

                secrets[key] = configuration[environmentVariable];
                if (secrets[key] != null)
                {
                    logMessages.Add(log => log.Debug("Value of {secretKey} satisfied by environment variable", key));
                    continue;
                }

                secrets[key] = defaultValue;
                if (secrets[key] != null)
                {
                    logMessages.Add(log => log.Warning("Value of {secretKey} satisfied by default value", key));
                }
                else
                {
                    logMessages.Add(
                        log =>
                        log.Error(
                            "Cannot find a value in secrets.json for {secretKey} or a value for the environment variable named {environmentVariable} and no default value was specified.",
                            key,
                            environmentVariable));
                }
            }

            return builder.AddInMemoryCollection(secrets);
        }

        public static void ValidateEnvironmentSecrets(this IConfigurationRoot configuration, ILogger log)
        {
            logMessages.ForEach(logMessage => logMessage(log));

            foreach (var (key, environmentVariable, defaultValue) in EnvironmentSecretConfig)
            {
                if (configuration.GetValue<string>(key) == null)
                {
                    throw new InvalidOperationException("Validation of environment secrets failed.");
                }
            }
        }
    }
}