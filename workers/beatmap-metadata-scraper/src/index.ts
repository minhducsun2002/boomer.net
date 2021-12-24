addEventListener('fetch', event => {
    event.respondWith(handleRequest(event.request))
})

declare var token : KVNamespace;
type HTMLRewriterElementContentHandler = Parameters<HTMLRewriter['on']>[1];

async function getToken() {
    let ret = await token.get('token');
    if (!ret) {
        let id = await token.get('id');
        let secret = await token.get('secret');
        let response = await fetch('https://osu.ppy.sh/oauth/token', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                client_id: +id!,
                client_secret: secret,
                grant_type: 'client_credentials',
                scope: 'public'
            })
        });

        let body: { expires_in: number, access_token: string } = await response.json();
        await token.put('token', body.access_token, {
            expirationTtl: body.expires_in - 100
        });
    }
    return await token.get('token');
}

async function handleRequest(req : Request) {
    let url = new URL(req.url);
    let isSet = url.searchParams.get('s') === '1';
    let { pathname } = url;
    let ids = pathname.split('/').filter(Boolean)[0].split(',');

    if (ids.length > 5 || ids.length < 1) {
        return new Response("give at most 5 and at least 1 beatmap(set) IDs", {
            status: 400
        });
    }
    let o = await Promise.all(
        ids.map(async id => {
            let s = await fetch(`https://osu.ppy.sh/${isSet ? 'beatmapsets' : 'beatmaps'}/${id}`)

            if (s.status === 429) {
                let token = await getToken();
                let setid = id;
                if (!isSet) {
                    setid = await fetch(`https://storage.ripple.moe/api/b/${id}`)
                        .then(r => r.json()).then((r: any) => r?.ParentSetID).catch(() => null);
                    if (!setid) {
                        setid = await fetch(`https://osu.ppy.sh/api/v2/beatmaps/${id}`, {
                            headers: {
                                'Authorization': `Bearer ${token}`
                            }
                        }).then(r => r.json()).then((r: any) => r.id);
                    }
                }

                return await fetch(`https://osu.ppy.sh/api/v2/beatmapsets/${id}`, {
                    headers: {
                        'Authorization': `Bearer ${token}`
                    }
                }).then(res => res.json())
            }

            let json = '';
            class Handler implements HTMLRewriterElementContentHandler {
                text(text : Text) {
                    json += text.text;
                }
            }

            await new HTMLRewriter().on('#json-beatmapset', new Handler()).transform(s).text();
            if (s.status != 200) {
                throw new Error(`Fetching id ${id} failed : status was ${s.status}`);
            }
            return JSON.parse(json);
        })
    )

    return new Response(JSON.stringify(o), {
        headers: {
            'Content-Type': 'application/json'
        }
    });
}