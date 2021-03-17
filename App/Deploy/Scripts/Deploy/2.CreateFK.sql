alter table tree 
add constraint fk_tree_formula_id 
	foreign key(formula_id) 
		references  formula(id) 
		on delete no action on update no action;

alter table tree_item 
add constraint fk_tree_item_tree_id 
	foreign key(tree_id) 
		references  tree(id) 
		on delete no action on update no action;

alter table tree_item 
add constraint fk_tree_item_parent_id 
	foreign key(parent_id) 
		references  tree_item(id) 
		on delete no action on update no action;