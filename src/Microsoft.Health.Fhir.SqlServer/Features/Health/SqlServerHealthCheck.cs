﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.SqlServer.Configs;
using Microsoft.Health.Fhir.SqlServer.Features.Storage;

namespace Microsoft.Health.Fhir.SqlServer.Features.Health
{
    public class SqlServerHealthCheck : IHealthCheck
    {
        private SqlServerDataStoreConfiguration _configuration;
        private ILogger<SqlServerFhirDataStore> _logger;

        public SqlServerHealthCheck(SqlServerDataStoreConfiguration configuration, ILogger<SqlServerFhirDataStore> logger)
        {
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            EnsureArg.IsNotNull(logger, nameof(logger));

            _configuration = configuration;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.ConnectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    SqlCommand command = connection.CreateCommand();
                    command.CommandText = "select @@DBTS";

                    await command.ExecuteScalarAsync(cancellationToken);

                    return HealthCheckResult.Healthy("Successfully connected to the data store.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to connect to the data store.");
                return HealthCheckResult.Unhealthy("Failed to connect to the data store.");
            }
        }
    }
}