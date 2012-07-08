
CREATE SEQUENCE public.account_persona_id_seq;

CREATE TABLE public.account (
                persona_id INTEGER NOT NULL DEFAULT nextval('public.account_persona_id_seq'),
                username VARCHAR(20) NOT NULL,
                passwd VARCHAR(32) NOT NULL,
                status OTHER NOT NULL,
                CONSTRAINT account_pk PRIMARY KEY (persona_id)
);
COMMENT ON COLUMN public.account.status IS 'TYPE charp_account_status';


ALTER SEQUENCE public.account_persona_id_seq OWNED BY public.account.persona_id;

CREATE UNIQUE INDEX account_idx
 ON public.account
 ( username );

CREATE SEQUENCE public.error_log_error_log_id_seq;

CREATE TABLE public.error_log (
                error_log_id INTEGER NOT NULL DEFAULT nextval('public.error_log_error_log_id_seq'),
                timestamp TIMESTAMP NOT NULL,
                error_code OTHER NOT NULL,
                username VARCHAR NOT NULL,
                ip_addr OTHER NOT NULL,
                res VARCHAR NOT NULL,
                msg VARCHAR NOT NULL,
                params OTHER NOT NULL,
                CONSTRAINT error_log_pk PRIMARY KEY (error_log_id)
);
COMMENT ON COLUMN public.error_log.error_code IS 'TYPE charp_error_code';
COMMENT ON COLUMN public.error_log.ip_addr IS 'TYPE INET';
COMMENT ON COLUMN public.error_log.params IS 'TYPE VARCHAR ARRAY';


ALTER SEQUENCE public.error_log_error_log_id_seq OWNED BY public.error_log.error_log_id;

CREATE TABLE public.request (
                request_id VARCHAR(64) NOT NULL,
                persona_id INTEGER NOT NULL,
                timestamp TIMESTAMP NOT NULL,
                ip_addr OTHER NOT NULL,
                proname VARCHAR NOT NULL,
                params VARCHAR(1024) NOT NULL,
                CONSTRAINT request_pk PRIMARY KEY (request_id)
);
COMMENT ON COLUMN public.request.ip_addr IS 'TYPE INET';
COMMENT ON COLUMN public.request.proname IS 'Nombre del remote procedure que se va a llamar.';


ALTER TABLE public.request ADD CONSTRAINT account_request_fk
FOREIGN KEY (persona_id)
REFERENCES public.account (persona_id)
ON DELETE NO ACTION
ON UPDATE NO ACTION
NOT DEFERRABLE;
