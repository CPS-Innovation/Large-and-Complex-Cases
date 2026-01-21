START TRANSACTION;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM large_complex_cases."__EFMigrationsHistory" WHERE "MigrationId" = '20250618064158_ActivityLogIndexes') THEN
    DROP INDEX large_complex_cases.idx_activity_log_action_type;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM large_complex_cases."__EFMigrationsHistory" WHERE "MigrationId" = '20250618064158_ActivityLogIndexes') THEN
    DROP INDEX large_complex_cases.idx_activity_log_resource_id;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM large_complex_cases."__EFMigrationsHistory" WHERE "MigrationId" = '20250618064158_ActivityLogIndexes') THEN
    DROP INDEX large_complex_cases.idx_activity_log_timestamp;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM large_complex_cases."__EFMigrationsHistory" WHERE "MigrationId" = '20250618064158_ActivityLogIndexes') THEN
    DROP INDEX large_complex_cases.idx_activity_log_user_name;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM large_complex_cases."__EFMigrationsHistory" WHERE "MigrationId" = '20250618064158_ActivityLogIndexes') THEN
    DELETE FROM large_complex_cases."__EFMigrationsHistory"
    WHERE "MigrationId" = '20250618064158_ActivityLogIndexes';
    END IF;
END $EF$;
COMMIT;

