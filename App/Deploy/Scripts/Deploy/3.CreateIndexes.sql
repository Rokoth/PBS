create unique index uidx_tree_name 
	on tree("name") where not is_deleted;


create unique index uidx_tree_item_name 
	on tree_item("name", parent_id) where not is_deleted;


create unique index uidx_formula_name 
	on formula("name") where not is_deleted;
