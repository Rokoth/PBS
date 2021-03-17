create table tree(
	  id            varchar(50)   not null primary key
	, "name"        varchar(100)  not null
	, "description" varchar(1000) not null
	, formula_id    varchar(50)   not null
	, version_date  timestamp     not null	
	, is_deleted    boolean       not null
	, is_sync       boolean       not null
);

create table tree_item(

	  id            varchar(50)   not null primary key
	, "name"        varchar(100)  not null
	, "description" varchar(1000) not null
	, tree_id       varchar(50)   not null
	, parent_id     varchar(50)
	, is_leaf       boolean       not null
	, select_count  int           not null
	, "weight"      int           not null
	, add_fields    varchar 
	, version_date  timestamp     not null	
	, is_deleted    boolean       not null
	, is_sync       boolean       not null
);

create table formula(
	  id            varchar(50)   not null primary key
	, "name"        varchar(100)  not null
	, "text"        varchar(1000) not null	
	, is_default    boolean       not null
	, version_date  timestamptz   not null
	, is_deleted    boolean       not null
	, is_sync       boolean       not null
);

create table sync_conflict(
	  s_id                varchar   not null primary key
	, id                  varchar   not null
	, "table"             varchar   not null
	, server_version_date timestamp not null
	, server_is_deleted   boolean   not null
	, local_version_date  timestamp not null
	, local_is_deleted    boolean   not null
);

create table settings(	 
	  id            int           not null primary key
	, param_name    varchar(100)  not null
	, param_value   varchar(1000) not null	
);