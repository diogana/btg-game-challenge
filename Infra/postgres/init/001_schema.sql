CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004161014_InitialSchema') THEN
    CREATE TABLE catalog_import_runs (
        id uuid NOT NULL,
        file_hash text NOT NULL,
        parser_version text NOT NULL,
        status text NOT NULL,
        started_at timestamp with time zone NOT NULL,
        finished_at timestamp with time zone,
        analyzed integer NOT NULL,
        imported integer NOT NULL,
        duplicates integer NOT NULL,
        rejected integer NOT NULL,
        warnings integer NOT NULL,
        CONSTRAINT "PK_catalog_import_runs" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004161014_InitialSchema') THEN
    CREATE TABLE friends (
        id uuid NOT NULL,
        name character varying(150) NOT NULL,
        email character varying(254),
        is_active boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_friends" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004161014_InitialSchema') THEN
    CREATE TABLE games (
        id uuid NOT NULL,
        title character varying(300) NOT NULL,
        platforms text[] NOT NULL,
        genres text[] NOT NULL,
        developer text,
        release_date date,
        rating numeric(6,2),
        votes bigint,
        source_price numeric(12,2),
        source_url text,
        is_active boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_games" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004161014_InitialSchema') THEN
    CREATE TABLE loans (
        id uuid NOT NULL,
        game_id uuid NOT NULL,
        friend_id uuid NOT NULL,
        loaned_at timestamp with time zone NOT NULL,
        returned_at timestamp with time zone,
        CONSTRAINT "PK_loans" PRIMARY KEY (id),
        CONSTRAINT ck_return_date CHECK (returned_at IS NULL OR returned_at >= loaned_at),
        CONSTRAINT "FK_loans_friends_friend_id" FOREIGN KEY (friend_id) REFERENCES friends (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_loans_games_game_id" FOREIGN KEY (game_id) REFERENCES games (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004161014_InitialSchema') THEN
    CREATE INDEX "IX_catalog_import_runs_file_hash_parser_version" ON catalog_import_runs (file_hash, parser_version) WHERE status = 'Completed';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004161014_InitialSchema') THEN
    CREATE INDEX "IX_friends_name" ON friends (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004161014_InitialSchema') THEN
    CREATE UNIQUE INDEX "IX_games_source_url" ON games (source_url) WHERE source_url IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004161014_InitialSchema') THEN
    CREATE INDEX "IX_games_title" ON games (title);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004161014_InitialSchema') THEN
    CREATE INDEX "IX_loans_friend_id_loaned_at" ON loans (friend_id, loaned_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004161014_InitialSchema') THEN
    CREATE INDEX "IX_loans_loaned_at" ON loans (loaned_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004161014_InitialSchema') THEN
    CREATE UNIQUE INDEX ux_loans_active_game ON loans (game_id) WHERE returned_at IS NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004161014_InitialSchema') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261004161014_InitialSchema', '10.0.12');
    END IF;
END $EF$;
COMMIT;

