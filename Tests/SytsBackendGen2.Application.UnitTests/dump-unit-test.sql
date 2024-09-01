--
-- PostgreSQL database dump
--

-- Dumped from database version 16.1 (Debian 16.1-1.pgdg120+1)
-- Dumped by pg_dump version 16.0

-- Started on 2024-03-01 14:56:23

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

--
-- TOC entry 219 (class 1259 OID 16414)
-- Name: TestAndNesteds; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."TestAndNesteds" (
    "TestEntityId" integer NOT NULL,
    "TestNestedEntityId" integer NOT NULL
);


ALTER TABLE public."TestAndNesteds" OWNER TO postgres;

--
-- TOC entry 218 (class 1259 OID 16404)
-- Name: TestEntities; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."TestEntities" (
    "Id" integer NOT NULL,
    "Created" timestamp with time zone NOT NULL,
    "LastModified" timestamp with time zone NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Description" character varying(100) NOT NULL,
    "Date" date NOT NULL,
    "SomeCount" integer NOT NULL,
    "InnerEntityId" integer NOT NULL
);


ALTER TABLE public."TestEntities" OWNER TO postgres;

--
-- TOC entry 217 (class 1259 OID 16403)
-- Name: TestEntities_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

-- ALTER TABLE public."TestEntities" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    -- SEQUENCE NAME public."TestEntities_Id_seq"
    -- START WITH 1
    -- INCREMENT BY 1
    -- NO MINVALUE
    -- NO MAXVALUE
    -- CACHE 1
-- );


--
-- TOC entry 216 (class 1259 OID 16390)
-- Name: TestNestedEntities; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."TestNestedEntities" (
    "Id" integer NOT NULL,
    "Created" timestamp with time zone NOT NULL,
    "LastModified" timestamp with time zone NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Number" numeric NOT NULL
);


ALTER TABLE public."TestNestedEntities" OWNER TO postgres;

--
-- TOC entry 215 (class 1259 OID 16389)
-- Name: TestNestedEntities_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

-- ALTER TABLE public."TestNestedEntities" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    -- SEQUENCE NAME public."TestNestedEntities_Id_seq"
    -- START WITH 1
    -- INCREMENT BY 1
    -- NO MINVALUE
    -- NO MAXVALUE
    -- CACHE 1
-- );


--
-- TOC entry 3374 (class 0 OID 0)
-- Dependencies: 217
-- Name: TestEntities_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

--SELECT pg_catalog.setval('public."TestEntities_Id_seq"', 1, false);


--
-- TOC entry 3375 (class 0 OID 0)
-- Dependencies: 215
-- Name: TestNestedEntities_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

--SELECT pg_catalog.setval('public."TestNestedEntities_Id_seq"', 1, false);


--
-- TOC entry 3217 (class 2606 OID 16418)
-- Name: TestAndNesteds TestEntities_In_TestNestedEntities_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TestAndNesteds"
    ADD CONSTRAINT "TestEntities_In_TestNestedEntities_pkey" PRIMARY KEY ("TestEntityId", "TestNestedEntityId");


--
-- TOC entry 3215 (class 2606 OID 16408)
-- Name: TestEntities TestEntities_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TestEntities"
    ADD CONSTRAINT "TestEntities_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 3213 (class 2606 OID 16402)
-- Name: TestNestedEntities TestNestedEntities_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TestNestedEntities"
    ADD CONSTRAINT "TestNestedEntities_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 3219 (class 2606 OID 16419)
-- Name: TestAndNesteds TestEntities_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TestAndNesteds"
    ADD CONSTRAINT "TestEntities_FK" FOREIGN KEY ("TestEntityId") REFERENCES public."TestEntities"("Id") ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3220 (class 2606 OID 16424)
-- Name: TestAndNesteds TestNestedEntities_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TestAndNesteds"
    ADD CONSTRAINT "TestNestedEntities_FK" FOREIGN KEY ("TestNestedEntityId") REFERENCES public."TestNestedEntities"("Id") ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3218 (class 2606 OID 16409)
-- Name: TestEntities TestNestedEntities_FK1; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TestEntities"
    ADD CONSTRAINT "TestNestedEntities_FK1" FOREIGN KEY ("InnerEntityId") REFERENCES public."TestNestedEntities"("Id");


-- Completed on 2024-03-01 14:56:23

--
-- PostgreSQL database dump complete
--

