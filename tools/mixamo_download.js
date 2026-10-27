// Mixamo Animation Downloader for NAAD Gate C
// Character: X Bot (ID: 2dee24f8-3b49-48af-b735-c6377509eaac)
//
// INSTRUCTIONS:
// 1. Browse mixamo.com and log in
// 2. Open JS console (F12 on chrome)
// 3. Copy and paste this entire script into the console
// 4. A new blank page will open and animations will start downloading
// 5. Keep the blank page open and keep clicking "Allow multiple downloads"
//
// TIP: To avoid clicking "Allow" repeatedly:
// chrome://settings/ > Advanced > Content > Automatic downloads > 
// uncheck "Do not allow any site to download multiple files automatically"

const character = '2dee24f8-3b49-48af-b735-c6377509eaac'  // X Bot

//=================================================================================================

const bearer = localStorage.access_token

const getAnimationList = (page) => {
    console.log('getAnimationList page=', page);
    const init = {
        method: 'GET',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearer}`,
            'X-Api-Key': 'mixamo2'
        }
    };

    const listUrl = `https://www.mixamo.com/api/v1/products?page=${page}&limit=96&order=&type=Motion%2CMotionPack&query=`;
    return fetch(listUrl, init).then((res) => res.json()).then((json) => json).catch(() => Promise.reject('Failed to download animation list'))
}

const getProduct = (animId, character) => {
    console.log('getProduct animId=', animId, ' character=', character);
    const init = {
        method: 'GET',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearer}`,
            'X-Api-Key': 'mixamo2'
        }
    };

    const productUrl = `https://www.mixamo.com/api/v1/products/${animId}?similar=0&character_id=${character}`;
    return fetch(productUrl, init).then((res) => res.json()).then((json) => json).catch(() => Promise.reject('Failed to download product details'))
}

const downloadAnimation = (animId, character, product_name) => {
    console.log('downloadAnimation animId=', animId, ' character=', character, ' prod name=', product_name);
    if (product_name.indexOf(',') > -1) {
        console.log('Skipping pack ', product_name);
        return Promise.resolve('Skip pack!');
    } else {
        return getProduct(animId, character)
                .then((json) => json.details.gms_hash)
                .then((gms_hash) => {
                    const pvals = gms_hash.params.map((param) => param[1]).join(',')
                    const _gms_hash = Object.assign({}, gms_hash, { params: pvals })
                    return exportAnimation(character, [_gms_hash], product_name)
                })
                .then((json) => monitorAnimation(character))
                .catch(() => Promise.reject("Unable to download animation " + animId))
    }
}

const downloadAnimLoop = (o) => {
    console.log('downloadAnimLoop');
    if (!o.anims.length) {
        return downloadAnimsInPage(o.currentPage + 1, o.totPages, o.character);
    }

    const head = o.anims[0];
    const tail = o.anims.slice(1);
    o.anims = tail;

    return downloadAnimation(head.id, o.character, head.description)
        .then(() => downloadAnimLoop(o))
        .catch(() => {
            console.log("Recovering from animation failed to download");
            return downloadAnimLoop(o)
        })
}

var downloadAnimsInPage = (page, totPages, character) => {
    console.log('downloadAnimsInPage page=', page, ' totPages', totPages, ' character=', character);
    if (page >= totPages) {
        console.log('All pages have been downloaded');
        return Promise.resolve('All pages have been downloaded');
    }

    return getAnimationList(page)
        .then((json) => (
            {
                anims: json.results,
                currentPage: json.pagination.page,
                totPages: json.pagination.num_pages,
                character
            }))
        .then((o) => downloadAnimLoop(o))
        .catch((e) => Promise.reject("Unable to download all animations error ", e))
}

const start = () => {
    console.log('Starting Mixamo animation download for character:', character);
    if (!character) {
        console.error("Please add a valid character ID at the beginning of the script");
        return
    }
    downloadAnimsInPage(1, 100, character);
}

const exportAnimation = (character_id, gmsHashArray, product_name) => {
    console.log('Exporting Anim:', character_id, " to file:", product_name)
    const exportUrl = 'https://www.mixamo.com/api/v1/animations/export'
    const exportBody = {
        character_id,
        gms_hash: gmsHashArray,
        preferences: { format: "fbx7", skin: "false", fps: "30", reducekf: "0" },
        product_name,
        type: "Motion"
    };
    const exportInit = {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearer}`,
            'X-Api-Key': 'mixamo2',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: JSON.stringify(exportBody)
    }
    return fetch(exportUrl, exportInit).then((res) => res.json()).then((json) => json)
}

const monitorAnimation = (characterId) => {
    const monitorUrl = `https://www.mixamo.com/api/v1/characters/${characterId}/monitor`;
    const monitorInit = {
        method: 'GET',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearer}`,
            'X-Api-Key': 'mixamo2'
        }
    };
    return fetch(monitorUrl, monitorInit)
        .then((res) => {
            switch (res.status) {
                case 404: {
                    const errorMsg = ('ERROR: Monitor got 404 error: ' + res.error + ' message=' + res.message);
                    console.error(errorMsg);
                    throw new Error(errorMsg);
                }
                case 202:
                case 200: {
                    return res.json()
                }
                default:
                    throw new Error('Response not handled', res);
            }
        }).then((msg) => {
            switch (msg.status) {
                case 'completed':
                    console.log('Downloading: ', msg.job_result);
                    downloadingTab.location.href = msg.job_result;
                    return msg.job_result;
                case 'processing':
                    console.log('Animation is processing... looping');
                    return monitorAnimation(characterId);
                case 'failed':
                default:
                    const errorMsg = ('ERROR: Monitor status:' + msg.status + ' message:' + msg.message + ' result:' + JSON.stringify(msg.job_result));
                    console.error(errorMsg);
                    throw new Error(errorMsg);
            }
        }).catch((e) => Promise.reject("Unable to monitor job for character " + characterId + e))
}

const downloadingTab = window.open('', '_blank');

start()
