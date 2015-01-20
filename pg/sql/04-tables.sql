-- This file is part of the CHARP project.
--
-- Copyright © 2011 - 2014
--   Free Software Foundation Europe, e.V.,
--   Talstrasse 110, 40217 Dsseldorf, Germany
--
-- Licensed under the EUPL V.1.1. See the file LICENSE.txt for copying conditions.


CREATE TABLE public.mime_type (
                mime_type_id INTEGER NOT NULL,
                type VARCHAR NOT NULL,
                name VARCHAR NOT NULL,
                extension VARCHAR NOT NULL,
                CONSTRAINT mime_type_pk PRIMARY KEY (mime_type_id)
);


CREATE UNIQUE INDEX mime_type_idx
 ON public.mime_type
 ( type );

CREATE TABLE public.hist_type (
                code imr_msg_code NOT NULL,
                type imr_msg_type NOT NULL,
                area imr_msg_area NOT NULL,
                CONSTRAINT hist_type_pk PRIMARY KEY (code)
);


CREATE TABLE public.message (
                code imr_msg_code NOT NULL,
                lang imr_lang NOT NULL,
                message VARCHAR NOT NULL,
                CONSTRAINT message_pk PRIMARY KEY (code, lang)
);


CREATE SEQUENCE public.country_country_id_seq;

CREATE TABLE public.country (
                country_id INTEGER NOT NULL DEFAULT nextval('public.country_country_id_seq'),
                ct_name VARCHAR NOT NULL,
                ct_abrev VARCHAR NOT NULL,
                CONSTRAINT country_pk PRIMARY KEY (country_id)
);
COMMENT ON COLUMN public.country.country_id IS 'CDH code';


ALTER SEQUENCE public.country_country_id_seq OWNED BY public.country.country_id;

CREATE SEQUENCE public.state_state_id_seq;

CREATE TABLE public.state (
                state_id INTEGER NOT NULL DEFAULT nextval('public.state_state_id_seq'),
                country_id INTEGER NOT NULL,
                st_name VARCHAR NOT NULL,
                st_abrev VARCHAR NOT NULL,
                CONSTRAINT state_pk PRIMARY KEY (state_id)
);


ALTER SEQUENCE public.state_state_id_seq OWNED BY public.state.state_id;

CREATE SEQUENCE public.muni_muni_id_seq;

CREATE TABLE public.muni (
                muni_id INTEGER NOT NULL DEFAULT nextval('public.muni_muni_id_seq'),
                state_id INTEGER NOT NULL,
                m_name VARCHAR NOT NULL,
                CONSTRAINT muni_pk PRIMARY KEY (muni_id)
);


ALTER SEQUENCE public.muni_muni_id_seq OWNED BY public.muni.muni_id;

CREATE SEQUENCE public.city_city_id_seq;

CREATE TABLE public.city (
                city_id INTEGER NOT NULL DEFAULT nextval('public.city_city_id_seq'),
                muni_id INTEGER NOT NULL,
                c_name VARCHAR NOT NULL,
                CONSTRAINT city_pk PRIMARY KEY (city_id)
);


ALTER SEQUENCE public.city_city_id_seq OWNED BY public.city.city_id;

CREATE SEQUENCE public.zipcode_zipcode_id_seq;

CREATE TABLE public.zipcode (
                zipcode_id INTEGER NOT NULL DEFAULT nextval('public.zipcode_zipcode_id_seq'),
                muni_id INTEGER NOT NULL,
                city_id INTEGER,
                z_code VARCHAR NOT NULL,
                CONSTRAINT zipcode_pk PRIMARY KEY (zipcode_id)
);


ALTER SEQUENCE public.zipcode_zipcode_id_seq OWNED BY public.zipcode.zipcode_id;

CREATE SEQUENCE public.asenta_asenta_id_seq;

CREATE TABLE public.asenta (
                asenta_id INTEGER NOT NULL DEFAULT nextval('public.asenta_asenta_id_seq'),
                zipcode_id INTEGER NOT NULL,
                a_name VARCHAR NOT NULL,
                a_type imr_asenta_type NOT NULL,
                CONSTRAINT asenta_pk PRIMARY KEY (asenta_id)
);


ALTER SEQUENCE public.asenta_asenta_id_seq OWNED BY public.asenta.asenta_id;

CREATE SEQUENCE public.inst_inst_id_seq;

CREATE TABLE public.inst (
                inst_id INTEGER NOT NULL DEFAULT nextval('public.inst_inst_id_seq'),
                inst_contact_persona_id INTEGER NOT NULL,
                country_id INTEGER NOT NULL,
                inst_name VARCHAR NOT NULL,
                inst_status imr_record_status NOT NULL,
                CONSTRAINT inst_pk PRIMARY KEY (inst_id)
);
COMMENT ON COLUMN public.inst.country_id IS 'CDH code';


ALTER SEQUENCE public.inst_inst_id_seq OWNED BY public.inst.inst_id;

CREATE SEQUENCE public.file_file_id_seq;

CREATE TABLE public.file (
                file_id INTEGER NOT NULL DEFAULT nextval('public.file_file_id_seq'),
                inst_id INTEGER NOT NULL,
                fname VARCHAR NOT NULL,
                created TIMESTAMP NOT NULL,
                mime_type_id INTEGER NOT NULL,
                CONSTRAINT file_pk PRIMARY KEY (file_id, inst_id)
);


ALTER SEQUENCE public.file_file_id_seq OWNED BY public.file.file_id;

CREATE UNIQUE INDEX photo_idx
 ON public.file
 ( fname );

CREATE INDEX file_idx
 ON public.file
 ( fname, created DESC );

CREATE SEQUENCE public.product_product_id_seq;

CREATE TABLE public.product (
                product_id INTEGER NOT NULL DEFAULT nextval('public.product_product_id_seq'),
                inst_id INTEGER NOT NULL,
                description VARCHAR NOT NULL,
                type imr_product_type NOT NULL,
                price INTEGER NOT NULL,
                CONSTRAINT product_pk PRIMARY KEY (product_id, inst_id)
);


ALTER SEQUENCE public.product_product_id_seq OWNED BY public.product.product_id;

CREATE SEQUENCE public.ailment_ailment_id_seq;

CREATE TABLE public.ailment (
                ailment_id INTEGER NOT NULL DEFAULT nextval('public.ailment_ailment_id_seq'),
                inst_id INTEGER NOT NULL,
                name VARCHAR NOT NULL,
                CONSTRAINT ailment_pk PRIMARY KEY (ailment_id, inst_id)
);


ALTER SEQUENCE public.ailment_ailment_id_seq OWNED BY public.ailment.ailment_id;

CREATE UNIQUE INDEX ailment_idx
 ON public.ailment
 ( name DESC );

CREATE SEQUENCE public.persona_persona_id_seq;

CREATE TABLE public.persona (
                persona_id INTEGER NOT NULL DEFAULT nextval('public.persona_persona_id_seq'),
                inst_id INTEGER NOT NULL,
                type imr_persona_type NOT NULL,
                prefix VARCHAR,
                name VARCHAR NOT NULL,
                paterno VARCHAR,
                materno VARCHAR,
                gender imr_gender,
                remarks VARCHAR,
                p_status imr_record_status NOT NULL,
                CONSTRAINT persona_pk PRIMARY KEY (persona_id, inst_id)
);


ALTER SEQUENCE public.persona_persona_id_seq OWNED BY public.persona.persona_id;

CREATE TABLE public.persona_photo (
                persona_id INTEGER NOT NULL,
                file_id INTEGER NOT NULL,
                inst_id INTEGER NOT NULL,
                CONSTRAINT persona_photo_pk PRIMARY KEY (persona_id, file_id, inst_id)
);


CREATE SEQUENCE public.address_address_id_seq;

CREATE TABLE public.address (
                address_id INTEGER NOT NULL DEFAULT nextval('public.address_address_id_seq'),
                inst_id INTEGER NOT NULL,
                persona_id INTEGER NOT NULL,
                asenta_id INTEGER,
                street VARCHAR NOT NULL,
                ad_type imr_address_type NOT NULL,
                CONSTRAINT address_pk PRIMARY KEY (address_id, inst_id)
);


ALTER SEQUENCE public.address_address_id_seq OWNED BY public.address.address_id;

CREATE TABLE public.fiscal (
                persona_id INTEGER NOT NULL,
                inst_id INTEGER NOT NULL,
                address_id INTEGER NOT NULL,
                code VARCHAR NOT NULL,
                alt_name VARCHAR,
                CONSTRAINT fiscal_pk PRIMARY KEY (persona_id, inst_id)
);
COMMENT ON COLUMN public.fiscal.alt_name IS 'In case fiscal data goes to another name (such as a company name)';


CREATE SEQUENCE public.email_email_id_seq;

CREATE TABLE public.email (
                email_id INTEGER NOT NULL DEFAULT nextval('public.email_email_id_seq'),
                inst_id INTEGER NOT NULL,
                persona_id INTEGER NOT NULL,
                email VARCHAR NOT NULL,
                type imr_email_type NOT NULL,
                system imr_email_system NOT NULL,
                remarks VARCHAR,
                CONSTRAINT email_pk PRIMARY KEY (email_id, inst_id)
);
COMMENT ON COLUMN public.email.remarks IS 'Short notes.';


ALTER SEQUENCE public.email_email_id_seq OWNED BY public.email.email_id;

CREATE SEQUENCE public.phone_phone_id_seq;

CREATE TABLE public.phone (
                phone_id INTEGER NOT NULL DEFAULT nextval('public.phone_phone_id_seq'),
                inst_id INTEGER NOT NULL,
                persona_id INTEGER NOT NULL,
                number VARCHAR NOT NULL,
                type imr_phone_type NOT NULL,
                remarks VARCHAR,
                ph_status imr_record_status NOT NULL,
                CONSTRAINT phone_pk PRIMARY KEY (phone_id, inst_id)
);


ALTER SEQUENCE public.phone_phone_id_seq OWNED BY public.phone.phone_id;

CREATE TABLE public.patient (
                persona_id INTEGER NOT NULL,
                inst_id INTEGER NOT NULL,
                birth DATE,
                sickness_remarks VARCHAR NOT NULL,
                alcohol BOOLEAN NOT NULL,
                alcohol_remarks VARCHAR NOT NULL,
                tobacco BOOLEAN NOT NULL,
                tobacco_remarks VARCHAR NOT NULL,
                drugs BOOLEAN NOT NULL,
                drugs_remarks VARCHAR NOT NULL,
                medication_remarks VARCHAR NOT NULL,
                diet_remarks VARCHAR NOT NULL,
                activity_remarks VARCHAR NOT NULL,
                CONSTRAINT patient_pk PRIMARY KEY (persona_id, inst_id)
);
COMMENT ON COLUMN public.patient.activity_remarks IS 'Actividad física';


CREATE SEQUENCE public.remedy_remedy_id_seq;

CREATE TABLE public.remedy (
                remedy_id INTEGER NOT NULL DEFAULT nextval('public.remedy_remedy_id_seq'),
                inst_id INTEGER NOT NULL,
                persona_id INTEGER NOT NULL,
                r_name VARCHAR NOT NULL,
                autosimile BIGINT NOT NULL,
                CONSTRAINT remedy_pk PRIMARY KEY (remedy_id, inst_id)
);


ALTER SEQUENCE public.remedy_remedy_id_seq OWNED BY public.remedy.remedy_id;

CREATE SEQUENCE public.referral_referral_id_seq;

CREATE TABLE public.referral (
                referral_id INTEGER NOT NULL DEFAULT nextval('public.referral_referral_id_seq'),
                inst_id INTEGER NOT NULL,
                patient_persona_id INTEGER NOT NULL,
                ref_type imr_referral_type NOT NULL,
                persona_id INTEGER NOT NULL,
                details VARCHAR NOT NULL,
                CONSTRAINT referral_pk PRIMARY KEY (referral_id, inst_id)
);


ALTER SEQUENCE public.referral_referral_id_seq OWNED BY public.referral.referral_id;

CREATE SEQUENCE public.analysis_analysis_id_seq;

CREATE TABLE public.analysis (
                analysis_id INTEGER NOT NULL DEFAULT nextval('public.analysis_analysis_id_seq'),
                inst_id INTEGER NOT NULL,
                persona_id INTEGER NOT NULL,
                ailment_id INTEGER,
                name VARCHAR,
                timestamp TIMESTAMP NOT NULL,
                height SMALLINT NOT NULL,
                weight SMALLINT NOT NULL,
                blood_pressure_sys SMALLINT NOT NULL,
                blood_pressure_dia SMALLINT NOT NULL,
                bmi SMALLINT NOT NULL,
                temp SMALLINT NOT NULL,
                heart_rate SMALLINT,
                resp_rate SMALLINT,
                blood_tension SMALLINT,
                remarks VARCHAR,
                level imr_level NOT NULL,
                CONSTRAINT analysis_pk PRIMARY KEY (analysis_id, inst_id)
);
COMMENT ON COLUMN public.analysis.blood_pressure_sys IS 'Systolic (maximum)';
COMMENT ON COLUMN public.analysis.blood_pressure_dia IS 'Diastolic (minimum)';
COMMENT ON COLUMN public.analysis.bmi IS 'Body - Mass Index';
COMMENT ON COLUMN public.analysis.blood_tension IS 'For the hypertense, max. systolic pressure registered?';


ALTER SEQUENCE public.analysis_analysis_id_seq OWNED BY public.analysis.analysis_id;

CREATE SEQUENCE public.code_code_id_seq;

CREATE TABLE public.code (
                code_id INTEGER NOT NULL DEFAULT nextval('public.code_code_id_seq'),
                inst_id INTEGER NOT NULL,
                c_name VARCHAR NOT NULL,
                code BIGINT NOT NULL,
                description VARCHAR NOT NULL,
                CONSTRAINT code_pk PRIMARY KEY (code_id, inst_id)
);


ALTER SEQUENCE public.code_code_id_seq OWNED BY public.code.code_id;

CREATE SEQUENCE public.teleheal_teleheal_id_seq;

CREATE TABLE public.teleheal (
                teleheal_id INTEGER NOT NULL DEFAULT nextval('public.teleheal_teleheal_id_seq'),
                inst_id INTEGER NOT NULL,
                persona_id INTEGER NOT NULL,
                code_id INTEGER NOT NULL,
                remedy_id INTEGER NOT NULL,
                analysis_id INTEGER NOT NULL,
                type imr_teleheal_type NOT NULL,
                program VARCHAR,
                starts TIMESTAMP NOT NULL,
                ends TIMESTAMP NOT NULL,
                CONSTRAINT teleheal_pk PRIMARY KEY (teleheal_id, inst_id)
);


ALTER SEQUENCE public.teleheal_teleheal_id_seq OWNED BY public.teleheal.teleheal_id;

CREATE TABLE public.analysis_code (
                analysis_id INTEGER NOT NULL,
                code_id INTEGER NOT NULL,
                inst_id INTEGER NOT NULL,
                value SMALLINT NOT NULL,
                level imr_level NOT NULL,
                suggested_levels imr_level ARRAY NOT NULL,
                CONSTRAINT analysis_code_pk PRIMARY KEY (analysis_id, code_id, inst_id)
);


CREATE TABLE public.remedy_code (
                remedy_id INTEGER NOT NULL,
                code_id INTEGER NOT NULL,
                inst_id INTEGER NOT NULL,
                power INTEGER NOT NULL,
                method imr_remedy_method NOT NULL,
                CONSTRAINT remedy_code_pk PRIMARY KEY (remedy_id, code_id, inst_id)
);


CREATE SEQUENCE public.category_category_id_seq;

CREATE TABLE public.category (
                category_id INTEGER NOT NULL DEFAULT nextval('public.category_category_id_seq'),
                inst_id INTEGER NOT NULL,
                parent_category_id INTEGER,
                name VARCHAR NOT NULL,
                CONSTRAINT category_pk PRIMARY KEY (category_id, inst_id)
);


ALTER SEQUENCE public.category_category_id_seq OWNED BY public.category.category_id;

CREATE TABLE public.category_code (
                category_id INTEGER NOT NULL,
                code_id INTEGER NOT NULL,
                inst_id INTEGER NOT NULL,
                CONSTRAINT category_code_pk PRIMARY KEY (category_id, code_id, inst_id)
);


CREATE TABLE public.account (
                persona_id INTEGER NOT NULL,
                inst_id INTEGER NOT NULL,
                username VARCHAR(20) NOT NULL,
                passwd VARCHAR(32) NOT NULL,
                account_type imr_account_type NOT NULL,
                status charp_account_status NOT NULL,
                CONSTRAINT account_pk PRIMARY KEY (persona_id, inst_id)
);


CREATE UNIQUE INDEX account_idx
 ON public.account
 ( username );

CREATE SEQUENCE public.hist_hist_id_seq;

CREATE TABLE public.hist (
                hist_id INTEGER NOT NULL DEFAULT nextval('public.hist_hist_id_seq'),
                inst_id INTEGER NOT NULL,
                subject_persona_id INTEGER NOT NULL,
                code imr_msg_code NOT NULL,
                persona_id INTEGER NOT NULL,
                timestamp TIMESTAMP NOT NULL,
                CONSTRAINT hist_pk PRIMARY KEY (hist_id, inst_id)
);


ALTER SEQUENCE public.hist_hist_id_seq OWNED BY public.hist.hist_id;

CREATE SEQUENCE public.hist_detail_hist_detail_id_seq;

CREATE TABLE public.hist_detail (
                hist_detail_id INTEGER NOT NULL DEFAULT nextval('public.hist_detail_hist_detail_id_seq'),
                inst_id INTEGER NOT NULL,
                hist_id INTEGER NOT NULL,
                pos SMALLINT NOT NULL,
                num INTEGER,
                str VARCHAR,
                CONSTRAINT hist_detail_pk PRIMARY KEY (hist_detail_id, inst_id)
);


ALTER SEQUENCE public.hist_detail_hist_detail_id_seq OWNED BY public.hist_detail.hist_detail_id;

CREATE SEQUENCE public.error_log_error_log_id_seq;

CREATE TABLE public.error_log (
                error_log_id INTEGER NOT NULL DEFAULT nextval('public.error_log_error_log_id_seq'),
                timestamp TIMESTAMP NOT NULL,
                error_code charp_error_code NOT NULL,
                username VARCHAR NOT NULL,
                ip_addr INET NOT NULL,
                res VARCHAR NOT NULL,
                msg VARCHAR NOT NULL,
                params VARCHAR ARRAY NOT NULL,
                CONSTRAINT error_log_pk PRIMARY KEY (error_log_id)
);


ALTER SEQUENCE public.error_log_error_log_id_seq OWNED BY public.error_log.error_log_id;

CREATE TABLE public.request (
                request_id VARCHAR(64) NOT NULL,
                inst_id INTEGER NOT NULL,
                persona_id INTEGER NOT NULL,
                timestamp TIMESTAMP NOT NULL,
                ip_addr INET NOT NULL,
                proname VARCHAR NOT NULL,
                params VARCHAR(1024) NOT NULL,
                CONSTRAINT request_pk PRIMARY KEY (request_id)
);
COMMENT ON COLUMN public.request.proname IS 'Nombre del remote procedure que se va a llamar.';


ALTER TABLE public.file ADD CONSTRAINT mime_type_photo_fk
FOREIGN KEY (mime_type_id)
REFERENCES public.mime_type (mime_type_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.message ADD CONSTRAINT hist_type_hist_msg_fk
FOREIGN KEY (code)
REFERENCES public.hist_type (code)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.hist ADD CONSTRAINT hist_type_hist_fk
FOREIGN KEY (code)
REFERENCES public.hist_type (code)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.state ADD CONSTRAINT country_state_fk
FOREIGN KEY (country_id)
REFERENCES public.country (country_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.inst ADD CONSTRAINT country_inst_fk
FOREIGN KEY (country_id)
REFERENCES public.country (country_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.muni ADD CONSTRAINT state_muni_fk
FOREIGN KEY (state_id)
REFERENCES public.state (state_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.zipcode ADD CONSTRAINT muni_zipcode_fk
FOREIGN KEY (muni_id)
REFERENCES public.muni (muni_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.city ADD CONSTRAINT muni_city_fk
FOREIGN KEY (muni_id)
REFERENCES public.muni (muni_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.zipcode ADD CONSTRAINT city_zipcode_fk
FOREIGN KEY (city_id)
REFERENCES public.city (city_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.asenta ADD CONSTRAINT zipcode_asenta_fk
FOREIGN KEY (zipcode_id)
REFERENCES public.zipcode (zipcode_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.address ADD CONSTRAINT asenta_address_fk
FOREIGN KEY (asenta_id)
REFERENCES public.asenta (asenta_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.account ADD CONSTRAINT inst_account_fk
FOREIGN KEY (inst_id)
REFERENCES public.inst (inst_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.category ADD CONSTRAINT inst_category_fk
FOREIGN KEY (inst_id)
REFERENCES public.inst (inst_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.code ADD CONSTRAINT inst_code_fk
FOREIGN KEY (inst_id)
REFERENCES public.inst (inst_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.persona ADD CONSTRAINT inst_persona_fk
FOREIGN KEY (inst_id)
REFERENCES public.inst (inst_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.ailment ADD CONSTRAINT inst_ailment_fk
FOREIGN KEY (inst_id)
REFERENCES public.inst (inst_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.product ADD CONSTRAINT inst_product_fk
FOREIGN KEY (inst_id)
REFERENCES public.inst (inst_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.referral ADD CONSTRAINT inst_referral_fk
FOREIGN KEY (inst_id)
REFERENCES public.inst (inst_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.remedy ADD CONSTRAINT inst_remedy_fk
FOREIGN KEY (inst_id)
REFERENCES public.inst (inst_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.analysis ADD CONSTRAINT inst_analysis_fk
FOREIGN KEY (inst_id)
REFERENCES public.inst (inst_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.file ADD CONSTRAINT inst_photo_fk
FOREIGN KEY (inst_id)
REFERENCES public.inst (inst_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.persona_photo ADD CONSTRAINT photo_persona_photo_fk
FOREIGN KEY (inst_id, file_id)
REFERENCES public.file (inst_id, file_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.analysis ADD CONSTRAINT ailment_analysis_fk
FOREIGN KEY (inst_id, ailment_id)
REFERENCES public.ailment (inst_id, ailment_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.patient ADD CONSTRAINT persona_patient_fk
FOREIGN KEY (inst_id, persona_id)
REFERENCES public.persona (inst_id, persona_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.phone ADD CONSTRAINT persona_phone_fk
FOREIGN KEY (inst_id, persona_id)
REFERENCES public.persona (inst_id, persona_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.email ADD CONSTRAINT persona_email_fk
FOREIGN KEY (inst_id, persona_id)
REFERENCES public.persona (inst_id, persona_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.address ADD CONSTRAINT persona_address_fk
FOREIGN KEY (inst_id, persona_id)
REFERENCES public.persona (inst_id, persona_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.hist ADD CONSTRAINT persona_hist_fk
FOREIGN KEY (inst_id, subject_persona_id)
REFERENCES public.persona (inst_id, persona_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.account ADD CONSTRAINT persona_account_fk
FOREIGN KEY (inst_id, persona_id)
REFERENCES public.persona (inst_id, persona_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.inst ADD CONSTRAINT persona_inst_fk
FOREIGN KEY (inst_id, inst_contact_persona_id)
REFERENCES public.persona (inst_id, persona_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE public.fiscal ADD CONSTRAINT persona_fiscal_fk
FOREIGN KEY (inst_id, persona_id)
REFERENCES public.persona (inst_id, persona_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.referral ADD CONSTRAINT persona_referral_fk
FOREIGN KEY (inst_id, persona_id)
REFERENCES public.persona (inst_id, persona_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.persona_photo ADD CONSTRAINT persona_persona_photo_fk
FOREIGN KEY (inst_id, persona_id)
REFERENCES public.persona (inst_id, persona_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.fiscal ADD CONSTRAINT address_fiscal_fk
FOREIGN KEY (inst_id, address_id)
REFERENCES public.address (inst_id, address_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.analysis ADD CONSTRAINT patient_analysis_fk
FOREIGN KEY (inst_id, persona_id)
REFERENCES public.patient (inst_id, persona_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.teleheal ADD CONSTRAINT patient_teleheal_fk
FOREIGN KEY (inst_id, persona_id)
REFERENCES public.patient (inst_id, persona_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.referral ADD CONSTRAINT patient_referral_fk
FOREIGN KEY (inst_id, patient_persona_id)
REFERENCES public.patient (inst_id, persona_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.remedy ADD CONSTRAINT patient_remedy_fk
FOREIGN KEY (inst_id, persona_id)
REFERENCES public.patient (inst_id, persona_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.remedy_code ADD CONSTRAINT remedy_remedy_code_fk
FOREIGN KEY (remedy_id, inst_id)
REFERENCES public.remedy (remedy_id, inst_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.teleheal ADD CONSTRAINT remedy_teleheal_fk
FOREIGN KEY (remedy_id, inst_id)
REFERENCES public.remedy (remedy_id, inst_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.analysis_code ADD CONSTRAINT analysis_analysis_code_fk
FOREIGN KEY (analysis_id, inst_id)
REFERENCES public.analysis (analysis_id, inst_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.teleheal ADD CONSTRAINT analysis_teleheal_fk
FOREIGN KEY (analysis_id, inst_id)
REFERENCES public.analysis (analysis_id, inst_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.category_code ADD CONSTRAINT code_category_code_fk
FOREIGN KEY (inst_id, code_id)
REFERENCES public.code (inst_id, code_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.remedy_code ADD CONSTRAINT code_remedy_code_fk
FOREIGN KEY (inst_id, code_id)
REFERENCES public.code (inst_id, code_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.analysis_code ADD CONSTRAINT code_analysis_code_fk
FOREIGN KEY (inst_id, code_id)
REFERENCES public.code (inst_id, code_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.teleheal ADD CONSTRAINT code_teleheal_fk
FOREIGN KEY (inst_id, code_id)
REFERENCES public.code (inst_id, code_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.category ADD CONSTRAINT category_category_fk
FOREIGN KEY (inst_id, parent_category_id)
REFERENCES public.category (inst_id, category_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.category_code ADD CONSTRAINT category_category_code_fk
FOREIGN KEY (inst_id, category_id)
REFERENCES public.category (inst_id, category_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.request ADD CONSTRAINT account_request_fk
FOREIGN KEY (inst_id, persona_id)
REFERENCES public.account (inst_id, persona_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.hist ADD CONSTRAINT account_hist_fk
FOREIGN KEY (inst_id, persona_id)
REFERENCES public.account (inst_id, persona_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;

ALTER TABLE public.hist_detail ADD CONSTRAINT hist_hist_detail_fk
FOREIGN KEY (hist_id, inst_id)
REFERENCES public.hist (hist_id, inst_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;
