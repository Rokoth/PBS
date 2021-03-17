create extension if not exists "uuid-ossp";

create table if not exists tree(
	  id            uuid          not null default uuid_generate_v4() primary key
	, "name"        varchar(100)  not null
	, "description" varchar(1000) not null
	, formula_id    uuid          not null
	, version_date  timestamptz   not null default now()
	, is_deleted    boolean       not null
);

create table if not exists tree_item(
	  id            uuid          not null default uuid_generate_v4() primary key
	, "name"        varchar(100)  not null
	, "description" varchar(1000) not null
	, tree_id       uuid          not null
	, parent_id     uuid
	, is_leaf       boolean       not null
	, select_count  int           not null  default 0
	, "weight"      int           not null  default 1
	, add_fields    jsonb 
	, version_date  timestamptz   not null default now()
	, is_deleted    boolean       not null
);

create table if not exists formula(
	  id            uuid          not null default uuid_generate_v4() primary key
	, "name"        varchar(100)  not null
	, "text"        varchar(1000) not null	
	, is_default    boolean       not null
	, version_date  timestamptz   not null default now()
	, is_deleted    boolean       not null
);

create table if not exists h_formula(
	  h_id          bigserial     primary key
	, id            uuid          null 
	, "name"        varchar(100)  null
	, "text"        varchar(1000) null	
	, is_default    boolean       null
	, version_date  timestamptz   null
	, is_deleted    boolean       null
	, change_date   timestamptz   not null default now()
	, "user_id"     varchar(100)  null
);

create table if not exists h_tree_item(
	  h_id          bigserial     primary key
	, id            uuid          null
	, "name"        varchar(100)  null
	, "description" varchar(1000) null
	, tree_id       uuid          null
	, parent_id     uuid          null
	, is_leaf       boolean       null
	, select_count  int           null
	, "weight"      int           null
	, add_fields    jsonb         null
	, version_date  timestamptz   null
	, is_deleted    boolean       null
	, change_date   timestamptz   not null default now()
	, "user_id"     varchar(100)  null
);

create table if not exists h_tree(
      h_id          bigserial     primary key
	, id            uuid          null
	, "name"        varchar(100)  null
	, "description" varchar(1000) null
	, formula_id    uuid          null
	, version_date  timestamptz   null
	, is_deleted    boolean       null
	, change_date   timestamptz   not null default now()
	, "user_id"     varchar(100)  null
);

create table if not exists settings(	 
	  id            int           not null primary key
	, param_name    varchar(100)  not null
	, param_value   varchar(1000) not null	
);