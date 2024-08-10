import json

def extract_uuids_and_titles(options_data, titles_data):
    category_uuids = {}

    for category in options_data['categoryList']:
        category_name = category['name']
        uuids_and_titles = []

        for option in category['options']:
            if 'uuid' in option:
                uuid = option['uuid']
                title = titles_data.get(uuid, {}).get('title', 'No title found')
                uuids_and_titles.append((uuid, title))

        category_uuids[category_name] = uuids_and_titles

    return category_uuids

def load_json_file(filename):
    try:
        with open(filename, 'r') as file:
            return json.load(file)
    except FileNotFoundError:
        print(f"Error: The file '{filename}' was not found.")
        exit(1)
    except json.JSONDecodeError:
        print(f"Error: The file '{filename}' contains invalid JSON.")
        exit(1)

# Load JSON data from files
options_data = load_json_file('c_options.json')
titles_data = load_json_file('uuid_titles.json')

# Extract UUIDs and titles
result = extract_uuids_and_titles(options_data, titles_data)

# Print the result
for category, uuids_and_titles in result.items():
    print(f"\npublic static class {category}Ids\n{{")
    for uuid, title in uuids_and_titles:
        print(f"    public const string {title.replace(' ', '_')} = \"{uuid}\";")
    print("}\n")