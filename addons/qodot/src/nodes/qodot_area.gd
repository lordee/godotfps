class_name QodotArea, 'res://addons/qodot/icons/icon_qodot_node.svg'
extends Area

export(Dictionary) var properties

func get_class():
	return 'QodotArea'

func add_child_editor(child):
	add_child(child)

	if not is_inside_tree():
		print("Not inside tree")
		return

	var tree = get_tree()

	if not tree:
		print("Invalid tree")
		return

	var edited_scene_root = tree.get_edited_scene_root()
	if not edited_scene_root:
		print("Invalid edited scene root")
		return

	child.set_owner(edited_scene_root)
