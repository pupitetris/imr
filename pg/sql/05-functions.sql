-- Application-specific functions.
BEGIN TRANSACTION;


M4_FUNCTION( «imr_account_type_has_perm(_type imr_account_type, _perm imr_perm)»,
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
		  «TABLE( persona_id integer, type imr_account_type, username varchar, photo varchar, remarks varchar,
			  prefix varchar, name varchar, paterno varchar, materno varchar, status charp_account_status,
			  gender imr_gender )»,
		  STABLE, M4_DEFN(user), 'Get the list of all users for the user''s instance.', «

SELECT a.persona_id, a.account_type, a.username, f.fname, p.remarks,
       p.prefix, p.name, p.paterno, p.materno, a.status, p.gender
       FROM account AS a1
	    JOIN account AS a USING (inst_id)
	    JOIN persona AS p ON (a.persona_id = p.persona_id)
	    LEFT JOIN (SELECT persona_id, inst_id, fname
	    	      	      FROM (SELECT persona_id, inst_id, max(created) AS created
			      	   	   FROM persona_photo
					   	NATURAL JOIN file
					   GROUP BY inst_id, persona_id) AS q
				   NATURAL JOIN file) AS f
		      ON (a.inst_id = f.inst_id AND a.persona_id = f.persona_id)
       WHERE a1.persona_id = $1 AND a.status <> 'DELETED';
»);


M4_FUNCTION( «rp_user_remove(_uid charp_user_id, _persona_id integer)»,
	     boolean, VOLATILE, M4_DEFN(user), 'Mark an user as DELETED.', «
DECLARE
	_my_type imr_account_type;
	_user_type varchar;
BEGIN
	-- Can't remove yourself.
	IF _uid = _persona_id THEN PERFORM charp_raise('USERPERM'); END IF;

	SELECT a1.account_type, a2.account_type INTO _my_type, _user_type
	       FROM account AS a1 JOIN account AS a2 USING (inst_id)
	       WHERE a1.persona_id = _uid AND a2.persona_id = _persona_id;

	IF NOT FOUND THEN PERFORM charp_raise('NOTFOUND'); END IF;
	IF NOT imr_account_type_has_perm(_my_type, 'USER_DELETE') OR
	   NOT imr_account_type_has_perm(_my_type, _user_type::imr_perm) THEN
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
		  «TABLE(address_id integer, persona_id integer, street varchar, ad_type imr_address_type, asenta_id integer)»,
		  STABLE, M4_DEFN(user), 'Get addresses related to a given persona.', «

SELECT a.address_id, a.persona_id, a.street, a.ad_type, a.asenta_id
       FROM address AS a JOIN account AS ac USING (inst_id)
       WHERE ac.persona_id = $1 AND a.persona_id = $2;
»);


M4_SQL_PROCEDURE( «rp_persona_get_phones(_uid charp_user_id, _persona_id integer)»,
		  «TABLE(phone_id integer, persona_id integer, numbr varchar, p_type imr_phone_type, remarks varchar)»,
		  STABLE, M4_DEFN(user), 'Get phones related to a given persona.', «

SELECT p.phone_id, p.persona_id, p.numbr, p.type, p.remarks
       FROM phone AS p JOIN account AS ac USING (inst_id)
       WHERE ac.persona_id = $1 AND p.persona_id = $2 AND p.ph_status <> 'DELETED';
»);


M4_SQL_PROCEDURE( «rp_persona_get_emails(_uid charp_user_id, _persona_id integer)»,
		  «TABLE(email_id integer, persona_id integer, email varchar, e_type imr_email_type, system imr_email_system, remarks varchar)»,
		  STABLE, M4_DEFN(user), 'Get emails related to a given persona.', «

SELECT e.email_id, e.persona_id, e.email, e.type, e.system, e.remarks
       FROM email AS e JOIN account AS ac USING (inst_id)
       WHERE ac.persona_id = $1 AND e.persona_id = $2;
»);


M4_FUNCTION( «imr_file_create(_inst_id integer, _file_name varchar, _mime_type varchar)»,
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
	
	INSERT INTO file (file_id, inst_id, fname, created, mime_type_id)
	       VALUES(DEFAULT, _inst_id, _file_name || '.' || _extension, CURRENT_TIMESTAMP, _mime_type_id)
	       RETURNING file_id INTO _file_id;
	PERFORM charp_cmd('FILE_CREATE', _file_name);
	RETURN _file_id;
END »);


M4_PROCEDURE( «imr_persona_add_photo(_inst_id integer, _persona_id integer)»,
	      void, VOLATILE, M4_DEFN(user), 'Add a new photo to a given person.', «
DECLARE
	_file_id integer;
	_filename text;
BEGIN
	_filename := md5('radio' || _inst_id::text || _persona_id::text || CURRENT_TIMESTAMP);
	_file_id := imr_file_create(_inst_id, _filename, 'image/jpeg');
	PERFORM charp_cmd('OTHER', 'Image-CreatePersonaThumb', _filename);
	INSERT INTO persona_photo (persona_id, inst_id, file_id)
	       VALUES(_persona_id, _inst_id, _file_id);
END »);


M4_FUNCTION( «imr_user_can_edit_persona(_uid charp_user_id, _persona_id integer, _op_type varchar DEFAULT 'EDIT')»,
	     integer, STABLE, M4_DEFN(user), 'Raise exceptions if a given user can''t edit a persona. op_type can be CREATE EDIT or DELETE. Return the instance ID for efficiency.', «
DECLARE
	_my_type imr_account_type;
	_persona_type imr_persona_type;
	_user_type varchar;
	_inst_id integer;
	_perm varchar;
BEGIN
	SELECT inst_id, account_type INTO _inst_id, _my_type FROM account WHERE persona_id = _uid;

	IF _uid = _persona_id THEN
	   IF NOT imr_account_type_has_perm(_my_type, ('USER_' || _op_type || '_SELF')::imr_perm) THEN PERFORM charp_raise('USERPERM'); END IF;
	   RETURN _inst_id;
	END IF;

	SELECT type INTO _persona_type FROM persona WHERE inst_id = _inst_id AND persona_id = _persona_id;
	IF NOT FOUND THEN PERFORM charp_raise('NOTFOUND'); END IF;

	IF NOT imr_account_type_has_perm(_my_type, (_persona_type::text || '_' || _op_type)::imr_perm) THEN
	   PERFORM charp_raise('USERPERM');
	END IF;

	IF _persona_type = 'USER' THEN
	   SELECT a2.account_type INTO _user_type
	   	  FROM account AS a1 JOIN account AS a2 USING (inst_id)
	    	  WHERE a1.persona_id = _uid AND a2.persona_id = _persona_id;

	   IF NOT FOUND THEN PERFORM charp_raise('NOTFOUND'); END IF;
	   IF NOT imr_account_type_has_perm (_my_type, _user_type::imr_perm) THEN
	      PERFORM charp_raise('USERPERM');
	   END IF;
	END IF;

	RETURN _inst_id;
END »);	      


M4_PROCEDURE( «rp_persona_add_photo(_uid charp_user_id, _persona_id integer)»,
	      void, STABLE, M4_DEFN(user), 'Add a new photo to a given user if permissions allow.', «
DECLARE
	_inst_id integer;
BEGIN
	_inst_id := imr_user_can_edit_persona(_uid, _persona_id);
	PERFORM imr_persona_add_photo(_inst_id, _persona_id);
END »);


M4_PROCEDURE( «rp_file_persona_get_photo(_uid charp_user_id, _persona_id integer, _thumbnail boolean)»,
	      «TABLE(mimetype text, filename text)»,
	      STABLE, M4_DEFN(user), 'Get current photo for a person from the repository.', «
DECLARE
	_inst_id integer;
	_filename text;
	_mimetype text;
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

	SELECT type, fname INTO _mimetype, _filename
	       FROM persona_photo
	       	    NATURAL JOIN file
	       	    NATURAL JOIN mime_type
	       WHERE inst_id = _inst_id AND persona_id = _persona_id
	       ORDER BY created DESC LIMIT 1;

	IF NOT FOUND THEN PERFORM charp_raise('EXIT', 'No photo'); END IF;
	mimetype := _mimetype;

	IF _thumbnail THEN
	   filename := ('M4_DEFN(image_repo_dir)/thumbs/' || _filename)::text;
	ELSE
	   filename := ('M4_DEFN(image_repo_dir)/' || _filename)::text;
	END IF;

	RETURN NEXT;
END »);


M4_FUNCTION( «rp_user_create(_uid charp_user_id, _username varchar, _passwd varchar, _account_type imr_account_type, _status charp_account_status)»,
	     «TABLE( persona_id integer, username varchar, account_type imr_account_type, status charp_account_status )»,
	     VOLATILE, M4_DEFN(user), 'Create an user with an empty persona.', «
DECLARE
	_my_type imr_account_type;
	_perm text;
	_inst_id integer;
	_persona_id integer;
BEGIN
	SELECT a.inst_id, a.account_type INTO _inst_id, _my_type FROM account AS a WHERE a.persona_id = _uid;

	IF NOT imr_account_type_has_perm(_my_type, 'USER_CREATE') OR
	   NOT imr_account_type_has_perm (_my_type, _account_type::text::imr_perm) THEN
	   PERFORM charp_raise('USERPERM');
	END IF;
	
	INSERT INTO persona (persona_id, inst_id, type, prefix, name, paterno, materno, gender, remarks, p_status)
	       VALUES(DEFAULT, _inst_id, 'USER', NULL, '', NULL, NULL, NULL, '', 'ACTIVE')
	       RETURNING persona.persona_id INTO _persona_id;

	INSERT INTO account (persona_id, inst_id, username, passwd, account_type, status)
	       VALUES(_persona_id, _inst_id, _username, _passwd, _account_type, _status);

	RETURN QUERY SELECT _persona_id, _username, _account_type, _status;
END »);


M4_PROCEDURE( «rp_user_update(_uid charp_user_id, _persona_id integer, _username varchar, _passwd varchar, _account_type imr_account_type, _status charp_account_status)»,
	      «TABLE( persona_id integer, username varchar, account_type imr_account_type, status charp_account_status )»,
	      VOLATILE, M4_DEFN(user), 'Update an account data. Password is not changed if it is empty string or NULL.', «
DECLARE
	_inst_id integer;
BEGIN
	_inst_id := imr_user_can_edit_persona(_uid, _persona_id);

	IF _passwd IS NOT NULL AND _passwd <> '' THEN
	   UPDATE account SET (username, passwd, account_type, status) = (_username, _passwd, _account_type, _status)
	          WHERE inst_id = _inst_id AND persona_id = _persona_id;
	ELSE
	   UPDATE account SET (username, account_type, status) = (_username, _account_type, _status)
	          WHERE inst_id = _inst_id AND persona_id = _persona_id;
	END IF;

	RETURN QUERY SELECT _persona_id, _username, _account_type, _status;
END »);


M4_PROCEDURE( «rp_persona_update(_uid charp_user_id, _persona_id integer, _prefix varchar, _name varchar, _paterno varchar, _materno varchar, _gender imr_gender, _remarks varchar)»,
	      «TABLE( persona_id integer, prefix varchar, name varchar, paterno varchar, materno varchar, gender imr_gender, remarks varchar )»,
	      VOLATILE, M4_DEFN(user), 'Update personal data.', «
DECLARE
	_inst_id integer;
BEGIN
	_inst_id := imr_user_can_edit_persona(_uid, _persona_id);

	UPDATE persona AS p SET (prefix, name, paterno, materno, gender, remarks) = (_prefix, _name, _paterno, _materno, _gender, _remarks)
	       WHERE p.inst_id = _inst_id AND p.persona_id = _persona_id;

	RETURN QUERY SELECT _persona_id, _prefix, _name, _paterno, _materno, _gender, _remarks;
END »);


M4_PROCEDURE( «rp_address_create(_uid charp_user_id, _persona_id integer, _asenta_id integer, _street varchar, _ad_type imr_address_type)»,
	      «TABLE( address_id integer, persona_id integer, asenta_id integer, street varchar, ad_type imr_address_type )»,
	      VOLATILE, M4_DEFN(user), 'Create an address record for a given persona.', «
DECLARE
	_inst_id integer;
	_address_id integer;
BEGIN
	_inst_id := imr_user_can_edit_persona(_uid, _persona_id);

	INSERT INTO address (address_id, inst_id, persona_id, asenta_id, street, ad_type)
	       VALUES(DEFAULT, _inst_id, _persona_id, _asenta_id, _street, _ad_type)
	       RETURNING address.address_id INTO _address_id;

	RETURN QUERY SELECT _address_id, _persona_id, _asenta_id, _street, _ad_type;
END »);


M4_PROCEDURE( «rp_address_update(_uid charp_user_id, _address_id integer, _persona_id integer, _asenta_id integer, _street varchar, _ad_type imr_address_type)»,
	      «TABLE( address_id integer, persona_id integer, asenta_id integer, street varchar, ad_type imr_address_type )»,
	      VOLATILE, M4_DEFN(user), 'Edit an address record for a given persona.', «
DECLARE
	_inst_id integer;
BEGIN
	_inst_id := imr_user_can_edit_persona(_uid, _persona_id);

	UPDATE address SET (asenta_id, street, ad_type) = (_asenta_id, _street, _ad_type)
	       WHERE inst_id = _inst_id AND persona_id = _persona_id AND address_id = _address_id;

	RETURN QUERY SELECT _address_id, _persona_id, _asenta_id, _street, _ad_type;
END »);


M4_PROCEDURE( «rp_phone_create(_uid charp_user_id, _persona_id integer, _numbr varchar, _type imr_phone_type, _remarks varchar)»,
	      «TABLE( phone_id integer, persona_id integer, numbr varchar, type imr_phone_type, remarks varchar )»,
	      VOLATILE, M4_DEFN(user), 'Create an phone record for a given persona.', «
DECLARE
	_inst_id integer;
	_phone_id integer;
BEGIN
	_inst_id := imr_user_can_edit_persona(_uid, _persona_id);

	INSERT INTO phone (phone_id, inst_id, persona_id, numbr, type, remarks, ph_status)
	       VALUES(DEFAULT, _inst_id, _persona_id, _numbr, _type, _remarks, 'ACTIVE')
	       RETURNING phone.phone_id INTO _phone_id;

	RETURN QUERY SELECT _phone_id, _persona_id, _numbr, _type, _remarks;
END »);


M4_PROCEDURE( «rp_phone_update(_uid charp_user_id, _phone_id integer, _persona_id integer, _numbr varchar, _type imr_phone_type, _remarks varchar)»,
	      «TABLE( phone_id integer, persona_id integer, numbr varchar, type imr_phone_type, remarks varchar )»,
	      VOLATILE, M4_DEFN(user), 'Edit an phone record for a given persona.', «
DECLARE
	_inst_id integer;
BEGIN
	_inst_id := imr_user_can_edit_persona(_uid, _persona_id);

	UPDATE phone SET (numbr, type, remarks) = (_numbr, _type, _remarks)
	       WHERE inst_id = _inst_id AND persona_id = _persona_id AND phone_id = _phone_id;

	RETURN QUERY SELECT _phone_id, _persona_id, _numbr, _type, _remarks;
END »);


M4_PROCEDURE( «rp_email_create(_uid charp_user_id, _persona_id integer, _email varchar, _type imr_email_type, _system imr_email_system, _remarks varchar)»,
	      «TABLE( email_id integer, persona_id integer, email varchar, type imr_email_type, system imr_email_system, remarks varchar )»,
	      VOLATILE, M4_DEFN(user), 'Create an email record for a given persona.', «
DECLARE
	_inst_id integer;
	_email_id integer;
BEGIN
	_inst_id := imr_user_can_edit_persona(_uid, _persona_id);

	INSERT INTO email (email_id, inst_id, persona_id, email, type, system, remarks)
	       VALUES(DEFAULT, _inst_id, _persona_id, _email, _type, _system, _remarks)
	       RETURNING email.email_id INTO _email_id;

	RETURN QUERY SELECT _email_id, _persona_id, _email, _type, _system, _remarks;
END »);


M4_PROCEDURE( «rp_email_update(_uid charp_user_id, _email_id integer, _persona_id integer, _email varchar, _type imr_email_type, _system imr_email_system, _remarks varchar)»,
	      «TABLE( email_id integer, email varchar, type imr_email_type, system imr_email_system, remarks varchar )»,
	      VOLATILE, M4_DEFN(user), 'Edit an email record for a given persona.', «
DECLARE
	_inst_id integer;
BEGIN
	_inst_id := imr_user_can_edit_persona(_uid, _persona_id);

	UPDATE email SET (email, type, system, remarks) = (_email, _type, _system, _remarks)
	       WHERE inst_id = _inst_id AND persona_id = _persona_id AND email_id = _email_id;

	RETURN QUERY SELECT _email_id, _persona_id, _email, _type, _system, _remarks;
END »);


M4_PROCEDURE( «rp_address_delete(_uid charp_user_id, _persona_id integer, _address_id integer)»,
	      void, VOLATILE, M4_DEFN(user), 'Remove an address record for a given persona.', «
DECLARE
	_inst_id integer;
BEGIN
	_inst_id := imr_user_can_edit_persona(_uid, _persona_id);

	DELETE FROM address WHERE inst_id = _inst_id AND persona_id = _persona_id AND address_id = _address_id;
END »);


M4_PROCEDURE( «rp_phone_delete(_uid charp_user_id, _persona_id integer, _phone_id integer)»,
	      void, VOLATILE, M4_DEFN(user), 'Remove an phone record for a given persona.', «
DECLARE
	_inst_id integer;
BEGIN
	_inst_id := imr_user_can_edit_persona(_uid, _persona_id);

	UPDATE phone SET ph_status = 'DELETED' WHERE inst_id = _inst_id AND persona_id = _persona_id AND phone_id = _phone_id;
END »);


M4_PROCEDURE( «rp_email_delete(_uid charp_user_id, _persona_id integer, _email_id integer)»,
	      void, VOLATILE, M4_DEFN(user), 'Remove an email record for a given persona.', «
DECLARE
	_inst_id integer;
BEGIN
	_inst_id := imr_user_can_edit_persona(_uid, _persona_id);

	DELETE FROM email WHERE inst_id = _inst_id AND persona_id = _persona_id AND email_id = _email_id;
END »);


COMMIT TRANSACTION;
