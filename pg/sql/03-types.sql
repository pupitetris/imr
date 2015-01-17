-- Application-specific data types.

-- These codes will grow as operations increase.
CREATE TYPE imr_msg_code AS ENUM (
       'USER_NEW'
);

CREATE TYPE imr_msg_type AS ENUM (
       'MONETARY',
       'INFO',
       'ERROR',
       'WARNING'
);

CREATE TYPE imr_msg_area AS ENUM (
       'SYSTEM',
       'PATIENT',
       'ANALYSIS',
       'REMEDY',
       'TREATMENT'
);

CREATE TYPE imr_lang AS ENUM (
       'EN',
       'ES'
);

CREATE TYPE imr_record_status AS ENUM (
       'ACTIVE',
       'DISABLED',
       'DELETED'
);

CREATE TYPE imr_asenta_type AS ENUM (
    'Aeropuerto',
    'Ampliación',
    'Barrio',
    'Campamento',
    'Centro',
    'Ciudad',
    'Club de Golf',
    'Colonia',
    'Condominio',
    'Congregación',
    'Conjunto Habitacional',
    'Ejido',
    'Equipamiento',
    'Estación',
    'Exhacienda',
    'Fábrica o Industria',
    'Finca',
    'Fraccionamiento',
    'Gran Usuario',
    'Granja',
    'Hacienda',
    'Ingenio',
    'Parque Industrial',
    'Poblado Comunal',
    'Pueblo',
    'Rancho o Ranchería',
    'Residencial',
    'Unidad Habitacional',
    'Villa',
    'Zona Comercial',
    'Zona Federal',
    'Zona Industrial',
    'Zona Rural'
);

CREATE TYPE imr_product_type AS ENUM (
       'ANALYSIS',
       'TREATMENT'
);

CREATE TYPE imr_persona_type AS ENUM (
       'INST',
       'USER',
       'PATIENT',
       'REFERRAL'
);

CREATE TYPE imr_gender AS ENUM (
       'MALE',
       'FEMALE'
);

CREATE TYPE imr_phone_type AS ENUM (
       'MOBILE',
       'NEXTEL',
       'HOME',
       'WORK'
);

CREATE TYPE imr_level AS ENUM (
       'FISICO',
       'EMOCIONAL',
       'MENTAL',
       'ESPIRITUAL 1',       
       'ESPIRITUAL 2',
       'ESPIRITUAL 3'
);

CREATE TYPE imr_teleheal_type AS ENUM (
       'SIMPLE',
       'PROG'
);

CREATE TYPE imr_remedy_method AS ENUM (
       'C', 'X', 'M', 'MM', 'LM'
);

CREATE TYPE imr_account_type AS ENUM (
       'ADMIN',
       'OPERATOR',
       'SUPERUSER'
);

CREATE TYPE imr_perm AS ENUM (
       'PATIENT_CREATE',
       'PATIENT_EDIT',
       'PATIENT_DELETE',
       'REFERRAL_CREATE',
       'REFERRAL_EDIT',
       'REFERRAL_DELETE',
       'USER_EDIT_SELF',
       'OPERATOR',

       'USER_CREATE',
       'USER_EDIT',
       'USER_DELETE',
       'ADMIN',

       'INST_EDIT',
       'SYSTEM_BACKUP',
       'SYSTEM_RESTORE',
       'USER_SET_STATUS',
       'USER_SET_ADMIN_LEVEL',
       'SUPERUSER'
);

CREATE TYPE imr_address_type AS ENUM (
       'HOME',
       'WORK',
       'FISCAL'
);

CREATE TYPE imr_email_type AS ENUM (
       'PERSONAL',
       'WORK'
);

CREATE TYPE imr_email_system AS ENUM (
       'STANDARD',
       'SKYPE'
);

CREATE TYPE imr_referral_type AS ENUM (
       'ADS',
       'WEBSITE',
       'SOCIAL_NETWORK',
       'FRIEND',
       'RELATIVE',
       'OTHER'
);
