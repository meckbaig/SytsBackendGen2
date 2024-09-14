
SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

CREATE TABLE public."Access" (
    "Id" integer NOT NULL,
    "Name" character varying(100),
    "LastModified" timestamp with time zone,
    "Created" timestamp with time zone
);


ALTER TABLE public."Access" OWNER TO postgres;


ALTER TABLE public."Access" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Access_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


CREATE TABLE public."Folders" (
    "SubChannelsJson" json,
    "AccessId" integer NOT NULL,
    "Color" character(7),
    "Icon" character varying,
    "Name" character varying(50) NOT NULL,
    "LastChannelsUpdate" timestamp with time zone,
    "ChannelsCount" integer NOT NULL,
    "LastVideoId" character varying(11),
    "YoutubeFolders" json,
    "LastModified" timestamp with time zone,
    "Created" timestamp with time zone,
    "UserId" integer NOT NULL,
    "Id" integer NOT NULL,
    "Guid" uuid,
    "LastVideosAccess" timestamp with time zone
);


ALTER TABLE public."Folders" OWNER TO postgres;

ALTER TABLE public."Folders" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Folders_Id_seq"
    START WITH 28
    INCREMENT BY 1
    MINVALUE 28
    NO MAXVALUE
    CACHE 28
);


CREATE SEQUENCE public."Folders_integer_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public."Folders_integer_id_seq" OWNER TO postgres;

ALTER SEQUENCE public."Folders_integer_id_seq" OWNED BY public."Folders"."Id";


CREATE TABLE public."Permissions" (
    "Id" integer NOT NULL,
    "Name" character varying(100) NOT NULL,
    "LastModified" timestamp with time zone NOT NULL,
    "Created" timestamp with time zone NOT NULL
);


ALTER TABLE public."Permissions" OWNER TO postgres;


CREATE TABLE public."PermissionsInRoles" (
    "PermissionId" integer NOT NULL,
    "RoleId" integer NOT NULL
);


ALTER TABLE public."PermissionsInRoles" OWNER TO postgres;

CREATE TABLE public."RefreshTokens" (
    "Id" integer NOT NULL,
    "Token" character varying(100) NOT NULL,
    "UserId" integer NOT NULL,
    "LastModified" timestamp with time zone NOT NULL,
    "Created" timestamp with time zone NOT NULL,
    "ExpirationDate" timestamp with time zone NOT NULL,
    "Invalidated" boolean DEFAULT false NOT NULL
);


ALTER TABLE public."RefreshTokens" OWNER TO postgres;

ALTER TABLE public."RefreshTokens" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."RefreshTokens_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


CREATE TABLE public."Roles" (
    "Id" integer NOT NULL,
    "Name" character varying(100) NOT NULL,
    "LastModified" timestamp with time zone,
    "Created" timestamp with time zone
);


ALTER TABLE public."Roles" OWNER TO postgres;

ALTER TABLE public."Roles" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Role_RoleId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


CREATE TABLE public."Users" (
    "RoleId" integer NOT NULL,
    "Email" character varying(320) NOT NULL,
    "LastChannelsUpdate" timestamp with time zone,
    "SubChannelsJson" json,
    "YoutubeId" character varying(24) NOT NULL,
    "Deleted" boolean DEFAULT false NOT NULL,
    "LastModified" timestamp with time zone,
    "Created" timestamp with time zone,
    "Id" integer NOT NULL
);


ALTER TABLE public."Users" OWNER TO postgres;

CREATE TABLE public."UsersCallsToFolders" (
    "UserId" integer NOT NULL,
    "FolderId" integer NOT NULL,
    "LastVideoId" character(11),
    "LastModified" timestamp with time zone,
    "Created" timestamp with time zone,
    "LastUserCall" timestamp with time zone
);


ALTER TABLE public."UsersCallsToFolders" OWNER TO postgres;


ALTER TABLE public."Users" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Users_Id_seq"
    START WITH 7
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);



INSERT INTO public."Access" OVERRIDING SYSTEM VALUE VALUES (1, 'Editable', '2024-09-02 08:46:37.477431+00', '2024-09-02 08:46:37.477431+00');
INSERT INTO public."Access" OVERRIDING SYSTEM VALUE VALUES (2, 'Private', '2024-09-02 08:46:37.477431+00', '2024-09-02 08:46:37.477431+00');
INSERT INTO public."Access" OVERRIDING SYSTEM VALUE VALUES (4, 'Public', '2024-09-11 15:27:51.861538+00', '-infinity');
INSERT INTO public."Access" OVERRIDING SYSTEM VALUE VALUES (3, 'LinkAccess', '2024-09-11 16:16:02.327791+00', '-infinity');

INSERT INTO public."Permissions" VALUES (1, 'PublicDataReader', '2024-09-02 10:37:03.44364+00', '2024-09-02 08:35:11.100806+00');
INSERT INTO public."Permissions" VALUES (2, 'PublcDataEditor', '2024-09-02 10:37:03.443641+00', '2024-09-02 08:35:11.100884+00');
INSERT INTO public."Permissions" VALUES (3, 'PrivateDataReader', '2024-09-02 10:37:03.443611+00', '2024-09-02 10:37:03.443574+00');
INSERT INTO public."Permissions" VALUES (4, 'PrivateDataEditor', '2024-09-02 10:37:03.44364+00', '2024-09-02 10:37:03.44364+00');

INSERT INTO public."RefreshTokens" OVERRIDING SYSTEM VALUE VALUES (260, 'JL0pMpjKhVKLuR3RN+sKx+U6+rf0RI46d2yaSgGxMnnPX9zzsJjXETTWDuhNRJffSo1s9ihr8fItbxKmIorISA==', 2, '2024-09-11 15:58:02.443518+00', '2024-09-11 15:27:24.515191+00', '2024-09-12 15:27:24.48138+00', false);

INSERT INTO public."PermissionsInRoles" VALUES (1, 1);
INSERT INTO public."PermissionsInRoles" VALUES (4, 1);
INSERT INTO public."PermissionsInRoles" VALUES (1, 2);
INSERT INTO public."PermissionsInRoles" VALUES (2, 2);
INSERT INTO public."PermissionsInRoles" VALUES (3, 2);
INSERT INTO public."PermissionsInRoles" VALUES (4, 2);

INSERT INTO public."Roles" OVERRIDING SYSTEM VALUE VALUES (1, 'RegularUser', '2024-09-02 08:47:44.662706+00', '2024-09-02 08:47:44.662706+00');
INSERT INTO public."Roles" OVERRIDING SYSTEM VALUE VALUES (2, 'Developer', '2024-09-02 08:47:44.662706+00', '2024-09-02 08:47:44.662706+00');

INSERT INTO public."Users" OVERRIDING SYSTEM VALUE VALUES (1, 'second@gmail.com', '2024-09-11 12:50:51.549981+00', '[]', 'UCraJ_Xeb5YEhkokOncb8Jw', false, '2024-09-11 12:50:51.56212+00', '2024-09-02 08:47:49.727516+00', 1);
INSERT INTO public."Users" OVERRIDING SYSTEM VALUE VALUES (1, 'first@gmail.com', '2024-09-11 13:01:14.871207+00', '[]', 'UCOByaobXOl6YYxUNBxSwe_7', false, '2024-09-11 13:01:14.871293+00', '2024-09-02 08:47:49.727516+00', 2);



SELECT pg_catalog.setval('public."Access_Id_seq"', 1, false);



SELECT pg_catalog.setval('public."Folders_Id_seq"', 839, true);


SELECT pg_catalog.setval('public."Folders_integer_id_seq"', 27, true);


SELECT pg_catalog.setval('public."RefreshTokens_Id_seq"', 262, true);


SELECT pg_catalog.setval('public."Role_RoleId_seq"', 1, false);


SELECT pg_catalog.setval('public."Users_Id_seq"', 12, true);


ALTER TABLE ONLY public."Access"
    ADD CONSTRAINT "Access_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."Folders"
    ADD CONSTRAINT "Folders_pkey" PRIMARY KEY ("Id");
    
ALTER TABLE ONLY public."UsersCallsToFolders"
    ADD CONSTRAINT "LastVideosInFolders_pkey" PRIMARY KEY ("UserId", "FolderId");

ALTER TABLE ONLY public."Permissions"
    ADD CONSTRAINT "Permission_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."PermissionsInRoles"
    ADD CONSTRAINT "Permissions_In_Roles_pkey" PRIMARY KEY ("PermissionId", "RoleId");

ALTER TABLE ONLY public."RefreshTokens"
    ADD CONSTRAINT "RefreshToken_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."Roles"
    ADD CONSTRAINT "Role_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "Users_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."Folders"
    ADD CONSTRAINT folders_unique UNIQUE ("Guid");

ALTER TABLE ONLY public."Folders"
    ADD CONSTRAINT "Access_FK" FOREIGN KEY ("AccessId") REFERENCES public."Access"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
        
ALTER TABLE ONLY public."UsersCallsToFolders"
    ADD CONSTRAINT "Folder_FK" FOREIGN KEY ("FolderId") REFERENCES public."Folders"("Id") ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public."PermissionsInRoles"
    ADD CONSTRAINT "Permission_FK" FOREIGN KEY ("PermissionId") REFERENCES public."Permissions"("Id") ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "RoleId" FOREIGN KEY ("RoleId") REFERENCES public."Roles"("Id");

ALTER TABLE ONLY public."PermissionsInRoles"
    ADD CONSTRAINT "Role_FK" FOREIGN KEY ("RoleId") REFERENCES public."Roles"("Id") ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public."Folders"
    ADD CONSTRAINT "Users_FK" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public."RefreshTokens"
    ADD CONSTRAINT "Users_FK" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON UPDATE CASCADE ON DELETE CASCADE;

REVOKE USAGE ON SCHEMA public FROM PUBLIC;
GRANT ALL ON SCHEMA public TO PUBLIC;

