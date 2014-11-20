-- Catalog COPYs.
BEGIN TRANSACTION;

	SET CONSTRAINTS ALL DEFERRED;

	M4_CATALOG(country);
	M4_CATALOG(state);
	M4_CATALOG(muni);
	M4_CATALOG(city);
	M4_CATALOG(zipcode);
	M4_CATALOG(asenta);

COMMIT TRANSACTION;
