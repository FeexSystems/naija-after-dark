#!/bin/bash
# Animation Organization Script for NAAD Gate C
# Moves animations from flat list to category subfolders

ANIM_DIR="/c/NAAD/naija-after-dark/game/unity/Assets/NAAD/Animation/Animations"
cd "$ANIM_DIR"

# Create subdirectories if they don't exist
mkdir -p Locomotion Social Phone Food Nightlife Seating Vehicle NPCWork

# Move files based on naming patterns
for file in *.fbx; do
    filename=$(basename "$file")
    lowername=$(echo "$filename" | tr '[:upper:]' '[:lower:]')
    
    # Locomotion
    if [[ "$lowername" =~ (idle|walk|run|jump|crouch|sprint) ]] && \
       ! [[ "$lowername" =~ (combat|rifle|sword|axe|box|fitness|workout|zombie|mutant|orc|ninja|dwarf|drunk|injured|carrying|wheelbarrow) ]]; then
        mv "$file" "Locomotion/"
        
    # Phone
    elif [[ "$lowername" =~ (phone|texting|cell|mobile) ]]; then
        mv "$file" "Phone/"
        
    # Social
    elif [[ "$lowername" =~ (wave|greeting|talk|laugh|thank|celebrate|angry|reject|question|pout|pray|secret|taunt) ]] && \
         ! [[ "$lowername =~ (rifle|sword|axe|fist|combat) ]]; then
        mv "$file" "Social/"
        
    # Food
    elif [[ "$lowername =~ (eat|drink|food|serving|order|vending) ]]; then
        mv "$file" "Food/"
        
    # Nightlife
    elif [[ "$lowername =~ (dance|dj|club|music|hip|samba|breakdance|thriller|disco) ]]; then
        mv "$file" "Nightlife/"
        
    # Seating
    elif [[ "$lowername =~ (sit|seat|bench|table|chair) ]]; then
        mv "$file" "Seating/"
        
    # Vehicle
    elif [[ "$lowername =~ (drive|car|bicycle|vehicle|wheelbarrow) ]]; then
        mv "$file" "Vehicle/"
        
    # NPC Work
    elif [[ "$lowername =~ (security|guard|police|bar|work|service|plant|build|clean) ]]; then
        mv "$file" "NPCWork/"
    fi
done

echo "Animation organization complete!"
echo "Remaining unsorted files:"
ls -1 *.fbx 2>/dev/null | wc -l
