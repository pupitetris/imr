-- Application-specific functions.


M4_FUNCTION( «account_type_has_perm(_type imr_account_type, _perm imr_perm)»,
	     boolean, IMMUTABLE, M4_DEFN(user), 'Check if a given type of account can perform an action.', «
DECLARE
	_level varchar;
BEGIN
	_level := _type;
	RETURN _perm::imr_perm = ANY (enum_range (null::imr_perm, _level::imr_perm));
END »);


M4_FUNCTION( rp_user_get_type(_uid charp_user_id),
	     imr_account_type, STABLE, M4_DEFN(user), 'Get the user level for UI configuration purposes.', «
DECLARE
	_res imr_account_type;
BEGIN
	SELECT account_type INTO _res FROM account WHERE persona_id = _uid;
	RETURN _res;
END »);


M4_SQL_PROCEDURE( rp_user_list_get(_uid charp_user_id),
		  «TABLE( persona_id integer, type imr_account_type, username varchar, picture varchar, remarks varchar,
			  prefix varchar, name varchar, paterno varchar, materno varchar, status charp_account_status,
			  gender imr_gender )»,
		  STABLE, M4_DEFN(user), 'Get the list of all users for the user''s instance.', «

SELECT a.persona_id, a.account_type, a.username, ''::varchar, p.remarks,
       p.prefix, p.name, p.paterno, p.materno, a.status, p.gender
       FROM account AS a1
	    JOIN account AS a USING (inst_id)
	    JOIN persona AS p ON (a.persona_id = p.persona_id)
       WHERE a1.persona_id = $1 AND a.status <> 'DELETED';
»);


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
	UPDATE persona SET p_status='DELETED' WHERE persona_id = _persona_id;

	RETURN TRUE;
END »);
	

M4_SQL_PROCEDURE( rp_get_states_by_inst(_uid charp_user_id),
		  «TABLE(state_id integer, st_name varchar, st_abrev varchar)»,
		  IMMUTABLE, M4_DEFN(user), 'Get states catalog for the user''s instance.', «

SELECT s.state_id, s.st_name, s.st_abrev
       FROM (SELECT country_id FROM account AS a NATURAL JOIN inst AS i WHERE a.persona_id = $1) AS q
	    NATURAL JOIN state AS s ORDER BY s.st_name;
»);


M4_SQL_PROCEDURE( rp_anon_get_zipcodes_by_state(_state_id integer),
		  «TABLE(zipcode_id integer, muni_id integer, z_code varchar)»,
		  IMMUTABLE, M4_DEFN(user), 'Get zipcode catalog for a given state.', «

SELECT z.zipcode_id, z.muni_id, z.z_code FROM zipcode AS z NATURAL JOIN muni WHERE state_id = $1 ORDER BY z.z_code;
»);


M4_SQL_PROCEDURE( rp_anon_get_munis_by_state(_state_id integer),
		  «TABLE(muni_id integer, m_name varchar)»,
		  IMMUTABLE, M4_DEFN(user), 'Get municipality catalog for a given state.', «

SELECT m.muni_id, m.m_name FROM muni AS m WHERE state_id = $1 ORDER BY m.m_name;
»);


M4_SQL_FUNCTION( «imr_asenta_fullname(_type imr_asenta_type, _name varchar, _city_name varchar)»,
		 text, IMMUTABLE, M4_DEFN(user), 'Generate the display name for an asenta.', «

SELECT $1 || ' ' || $2 || COALESCE(', ' || $3, '');
»);


M4_SQL_PROCEDURE( rp_anon_get_asentas_by_muni(_muni_id integer),
		  «TABLE(asenta_id integer, z_code varchar, fullname varchar)»,
		  IMMUTABLE, M4_DEFN(user), 'Get asenta catalog for a given municipality.', «

SELECT a.asenta_id, z.z_code, imr_asenta_fullname(a.a_type, a.a_name, c.c_name)
       FROM asenta AS a NATURAL JOIN zipcode AS z NATURAL LEFT JOIN city AS c
       WHERE z.muni_id = $1
       ORDER BY a.a_name;
»);


M4_SQL_PROCEDURE( «rp_persona_get_addresses(_uid charp_user_id, _persona_id integer)»,
		  «TABLE(address_id integer, street varchar, ad_type imr_address_type, asenta_id integer)»,
		  STABLE, M4_DEFN(user), 'Get addresses related to a given persona.', «

SELECT a.address_id, a.street, a.ad_type, a.asenta_id
       FROM address AS a JOIN account AS ac USING (inst_id)
       WHERE ac.persona_id = $1 AND a.persona_id = $2;
»);


M4_SQL_PROCEDURE( «rp_persona_get_phones(_uid charp_user_id, _persona_id integer)»,
		  «TABLE(phone_id integer, number varchar, p_type imr_phone_type, remarks varchar)»,
		  STABLE, M4_DEFN(user), 'Get phones related to a given persona.', «

SELECT p.phone_id, p.number, p.p_type, p.remarks
       FROM phone AS p JOIN account AS ac USING (inst_id)
       WHERE ac.persona_id = $1 AND p.persona_id = $2;
»);


M4_SQL_PROCEDURE( «rp_persona_get_emails(_uid charp_user_id, _persona_id integer)»,
		  «TABLE(email_id integer, email varchar, e_type imr_email_type, system imr_email_system, remarks varchar)»,
		  STABLE, M4_DEFN(user), 'Get emails related to a given persona.', «

SELECT e.email_id, e.email, e.e_type, e.system, e.remarks
       FROM email AS e JOIN account AS ac USING (inst_id)
       WHERE ac.persona_id = $1 AND e.persona_id = $2;
»);


M4_FUNCTION( «file_create(_inst_id integer, _file_name varchar, _mime_type varchar)»,
	     integer, VOLATILE, M4_DEFN(user), '', «
DECLARE
	_mime_type_id integer;
	_extension varchar;
	_file_id integer;
BEGIN
	SELECT mime_type_id, extension INTO _mime_type_id, _extension FROM mime_type WHERE type = _mime_type;
	IF NOT FOUND THEN
	   PERFORM charp_raise('ASSERT', 'MIME type is correct');
	END IF;
	
	INSERT INTO file VALUES(DEFAULT, _inst_id, _file_name || '.' || _extension, CURRENT_TIMESTAMP, _mime_type_id)
	       RETURNING file_id INTO _file_id;
	PERFORM charp_cmd('FILE_CREATE', _file_name);
	RETURN _file_id;
END »);


M4_PROCEDURE( «persona_add_photo(_inst_id integer, _persona_id integer)»,
	      void, VOLATILE, M4_DEFN(user), 'Add a new photo to a given person.', «
DECLARE
	_file_id integer;
BEGIN
	_file_id := file_create(_inst_id, md5(_inst_id::text || _persona_id::text || CURRENT_TIMESTAMP), 'image/jpeg');
	INSERT INTO persona_photo VALUES(_persona_id, _inst_id, _file_id);
END »);


M4_PROCEDURE( «rp_user_add_photo(_uid charp_user_id, _persona_id integer)»,
	      void, STABLE, M4_DEFN(user), 'Add a new photo to a given user if permissions allow.', «
DECLARE
	_my_type imr_account_type;
	_user_type varchar;
	_inst_id integer;
BEGIN
	IF _uid = _persona_id THEN
	   -- You have the right to change your own image. CHECK: or not?
	   SELECT inst_id INTO _inst_id FROM account WHERE persona_id = _uid;
	ELSE	   
	   SELECT a1.account_type, a2.account_type, inst_id INTO _my_type, _user_type, _inst_id
	   	  FROM account AS a1 JOIN account AS a2 USING (inst_id)
	       	  WHERE a1.persona_id = _uid AND a2.persona_id = _persona_id;

	   IF NOT FOUND THEN PERFORM charp_raise('NOTFOUND'); END IF;
	   IF NOT account_type_has_perm(_my_type, 'USER_EDIT') OR
	      NOT account_type_has_perm (_my_type, _user_type::imr_perm) THEN
	      PERFORM charp_raise('USERPERM');
	   END IF;
	END IF;

	PERFORM persona_add_photo(_inst_id, _persona_id);
END »);


M4_PROCEDURE( «rp_file_persona_get_photo(_uid charp_user_id, _persona_id integer)»,
	      «TABLE(mimetype text, filename text)»,
	      STABLE, M4_DEFN(user), 'Get current photo for a person from the repository.', «
DECLARE
	_inst_id integer;
	_fname varchar;
BEGIN
	IF _uid = _persona_id THEN
	   -- Trivial case.
	   SELECT inst_id INTO _inst_id FROM account WHERE persona_id = _uid;
	ELSE	   
	   SELECT inst_id INTO _inst_id
	   	  FROM account AS a JOIN persona AS p USING (inst_id)
	       	  WHERE a.persona_id = _uid AND p.persona_id = _persona_id;

	   IF NOT FOUND THEN PERFORM charp_raise('NOTFOUND'); END IF;
	END IF;

	SELECT fname INTO _fname FROM persona_photo NATURAL JOIN file
	       WHERE inst_id = _inst_id AND persona_id = _persona_id
	       ORDER BY created DESC LIMIT 1;

	IF NOT FOUND THEN PERFORM charp_raise('EXIT', 'No photo'); END IF;
	RETURN QUERY SELECT 'image/jpeg'::text, ('M4_DEFN(image_repo_dir)/' || _fname)::text;
END »);
