addEventListener('fetch', event => {
    event.respondWith(handleRequest(event.request))
})

type HTMLRewriterElementContentHandler = Parameters<HTMLRewriter['on']>[1];

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

            let json = '';
            class Handler implements HTMLRewriterElementContentHandler {
                text(text : Text) {
                    json += text.text;
                }
            }

            await new HTMLRewriter().on('#json-beatmapset', new Handler()).transform(s).text();
            return json;
        })
    )

    return new Response(JSON.stringify(JSON.parse(`[${o.join(',')}]`)), {
        headers: {
            'Content-Type': 'application/json'
        }
    });
}