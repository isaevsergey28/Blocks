#!/bin/sh
echo "Firebase upload symbols script"

# check build configuration
if [ "${CONFIGURATION}" != "Release" ]; then
    echo "Skip dsyms uploading."
    exit 0
fi

ATTEMPTS=10
DELAY=1
while [ $ATTEMPTS -gt 0 ]
do
    DSYM_DIR="${DWARF_DSYM_FOLDER_PATH}/${DWARF_DSYM_FILE_NAME}"
    if [ ! -d "${DSYM_DIR}" ];
    then
        echo "Missing directory ${DSYM_DIR}"
        ATTEMPTS=$((ATTEMPTS-2))
        sleep $DELAY
    else
        DSYM_PREV_SIZE=$(du -s "$DSYM_DIR" | awk '{print $1}')
        sleep $DELAY
        DSYM_SIZE=$(du -s "$DSYM_DIR" | awk '{print $1}')
    
        if [[ $DSYM_SIZE -gt 8 ]] && [[ $DSYM_SIZE -eq $DSYM_PREV_SIZE ]];
        then
            echo "Uploading dsyms: (${DSYM_SIZE})"
            "${PROJECT_DIR}/firebase-upload-symbols" -gsp "${PROJECT_DIR}/GoogleService-Info.plist" -p ios "${DSYM_DIR}"
            echo "Uploading complete."
            exit 0
        fi

        echo "Waiting for dsyms to be generated: (${DSYM_SIZE})"
        DSYM_PREV_SIZE=$DSYM_SIZE
        ATTEMPTS=$((ATTEMPTS-1))
        DELAY=5
    fi
done

echo "Failed to upload dsyms."
exit 1
