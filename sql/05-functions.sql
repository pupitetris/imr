-- Application-specific functions.

CREATE OR REPLACE FUNCTION rp_user_get_type(_uid charp_user_id)
  RETURNS imr_account_type AS
$BODY$
DECLARE
		_res imr_account_type;
BEGIN
		SELECT account_type INTO _res FROM account WHERE persona_id = _uid;
		RETURN _res;
END
$BODY$
  LANGUAGE plpgsql STABLE;
ALTER FUNCTION rp_user_get_type(_uid charp_user_id) OWNER TO M4_DEFN(user);
COMMENT ON FUNCTION rp_user_get_type(_uid charp_user_id) IS 'Get the user level for UI configuration purposes.';
