-- Application-specific functions.

M4_FUNCTION( rp_user_get_type(_uid charp_user_id), imr_account_type, 
			 STABLE, M4_DEFN(user), 'Get the user level for UI configuration purposes.', «
DECLARE
		_res imr_account_type;
BEGIN
		SELECT account_type INTO _res FROM account WHERE persona_id = _uid;
		RETURN _res;
END »);


M4_FUNCTION( rp_user_list_get(_uid charp_user_id),
			 «TABLE( persona_id integer, username varchar, prefix varchar, name varchar, 
			 		 paterno varchar, materno varchar, status charp_account_status )»,
			 STABLE, M4_DEFN(user), 'Get the list of all users for the user''s instance.', «
BEGIN
		RETURN QUERY SELECT a.persona_id, a.username, p.prefix, p.name, p.paterno, p.materno, a.status 
			   		 		FROM account AS a1 
			   		 			 JOIN account AS a USING (inst_id) 
								 JOIN persona AS p ON (a.persona_id = p.persona_id) 
							WHERE a1.persona_id = _uid;
END »);

