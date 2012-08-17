-- Application-specific functions.


M4_FUNCTION( «account_type_has_perm(_type imr_account_type, _perm imr_perm)», boolean,
			 IMMUTABLE, M4_DEFN(user), 'Check if a given type of account can perform an action.', «
DECLARE
		_level varchar;
BEGIN
		_level := _type;
		RETURN _perm::imr_perm = ANY (enum_range (null::imr_perm, _level::imr_perm));
END »);


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
							WHERE a1.persona_id = _uid AND a.status <> 'DELETED';
END »);


M4_FUNCTION( «rp_user_remove(_uid charp_user_id, _persona_id integer)»,
			 boolean, VOLATILE, M4_DEFN(user), 'Mark an user as DELETED.', «
DECLARE
		_my_type imr_account_type;
		_user_type varchar;
BEGIN
		IF _uid = _persona_id THEN PERFORM charp_raise('USERPERM'); END IF;

		SELECT a1.account_type, a2.account_type INTO _my_type, _user_type 
			   FROM account AS a1 JOIN account AS a2 USING (inst_id)
			   WHERE a1.persona_id = _uid AND a2.persona_id = _persona_id;

		IF NOT FOUND THEN PERFORM charp_raise('NOTFOUND'); END IF;
		IF NOT account_type_has_perm(_my_type, 'USER_DELETE') OR 
		   NOT account_type_has_perm (_my_type, _user_type::imr_perm) THEN 
		   PERFORM charp_raise('USERPERM');
		END IF;

		UPDATE account SET status='DELETED' WHERE persona_id = _persona_id;

		RETURN TRUE;
END »);
		

