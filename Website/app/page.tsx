'use client';

import { useEffect, useRef, useState } from 'react';

type PreviewBlock = { type: 'narration' | 'dialogue' | 'separator' | 'character'; speaker?: string; text?: string; character?: string };
type PreviewCharacter = { name: string; image: string };
type StoryPreview = { title: string; subtitle?: string; estimatedReadingMinutes?: number; characters?: Record<string, PreviewCharacter>; blocks: PreviewBlock[] };
type ReleaseStage = 'stable' | 'beta';
type Story = { id: string; title: string; genre: string; releaseStage: ReleaseStage; description: string; cover: string; accent: string; preview?: StoryPreview };
type ChannelManifest = { stories?: Record<string, string> };
type StoryCard = { storyId?: string; title?: string; genre?: string; releaseStage?: ReleaseStage; description?: string; cover?: string };

const contentRoot = '/content';
const manifestUrl = `${contentRoot}/kostroma-dev.json`;
const accents = ['#9aa995', '#86b7c6', '#79bed1', '#d0a76a', '#b49ac9'];

async function loadStories(): Promise<Story[]> {
  const manifestResponse = await fetch(manifestUrl, { cache: 'no-store' });
  if (!manifestResponse.ok) throw new Error('manifest');
  const manifest = await manifestResponse.json() as ChannelManifest;
  const releases = Object.entries(manifest.stories ?? {});
  if (!releases.length) throw new Error('empty');

  return Promise.all(releases.map(async ([storyId, version], index) => {
    const storyRoot = `${contentRoot}/stories/${encodeURIComponent(storyId)}/${encodeURIComponent(version)}`;
    const previewRoot = `${storyRoot}/preview`;
    const [cardResponse, previewResponse] = await Promise.all([
      fetch(`${storyRoot}/card.json`, { cache: 'no-store' }),
      fetch(`${previewRoot}/preview.json`, { cache: 'no-store' }),
    ]);
    if (!cardResponse.ok) throw new Error(storyId);
    const card = await cardResponse.json() as StoryCard;
    const rawPreview = previewResponse.ok ? await previewResponse.json() as StoryPreview : undefined;
    const preview = rawPreview ? {
      ...rawPreview,
      characters: Object.fromEntries(Object.entries(rawPreview.characters ?? {}).map(([id, character]) => [id, { ...character, image: `${previewRoot}/${character.image}` }])),
    } : undefined;
    return {
      id: card.storyId || storyId,
      title: card.title || storyId,
      genre: card.genre || '',
      releaseStage: card.releaseStage === 'beta' ? 'beta' : 'stable',
      description: card.description || '',
      cover: `${storyRoot}/${card.cover || 'cover.png'}`,
      accent: accents[index % accents.length],
      preview,
    };
  }));
}

export default function Home() {
  const [locked, setLocked] = useState(true);
  const [password, setPassword] = useState('');
  const [passwordFailed, setPasswordFailed] = useState(false);
  const [stories, setStories] = useState<Story[]>([]);
  const [active, setActive] = useState(0);
  const [failed, setFailed] = useState(false);
  const [webVersions, setWebVersions] = useState<Record<string, string>>({});
  const [previewStory, setPreviewStory] = useState<Story | null>(null);
  const [previewCharacters, setPreviewCharacters] = useState<{ current?: string; previous?: string }>({});
  const touchStart = useRef<number | null>(null);
  const previewCharacterCurrent = useRef<string | undefined>(undefined);
  const previewCharacterTimers = useRef<Array<ReturnType<typeof setTimeout>>>([]);

  useEffect(() => {
    setLocked(sessionStorage.getItem('kostroma-preview-access') !== 'granted');
    let cancelled = false;
    fetch('/player/config.json', { cache: 'no-store' }).then(async response => {
      if (!response.ok) return;
      const config = await response.json();
      if (cancelled || !config || typeof config !== 'object' || !('schema' in config)
        || config.schema !== 1 || !('player' in config) || !config.player
        || !('stories' in config) || !config.stories || typeof config.stories !== 'object') return;
      const versions: Record<string, string> = {};
      for (const [id, value] of Object.entries(config.stories ?? {})) {
        if (!value || typeof value !== 'object') continue;
        const entry = value as { version?: string; manifestUrl?: string };
        if (/^[a-z0-9]+(?:-[a-z0-9]+)*$/.test(id) && typeof entry.version === 'string'
          && /^[a-zA-Z0-9._-]+$/.test(entry.version) && entry.version !== '.' && entry.version !== '..'
          && entry.manifestUrl === `/content/stories/${id}/${entry.version}/webgl/release.json`) versions[id] = entry.version;
      }
      setWebVersions(versions);
    }).catch(() => { /* Optional WebGL release; existing previews/app remain available. */ });
    return () => { cancelled = true; };
  }, []);

  const unlock = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (password.trim().toLocaleLowerCase('ru-RU') !== 'огурец') {
      setPasswordFailed(true);
      return;
    }
    sessionStorage.setItem('kostroma-preview-access', 'granted');
    setLocked(false);
    setPassword('');
  };

  const goTo = (index: number) => {
    if (stories.length) {
      setPreviewStory(null);
      setActive((index + stories.length) % stories.length);
    }
  };

  const clearPreviewCharacterTimers = () => {
    previewCharacterTimers.current.forEach(clearTimeout);
    previewCharacterTimers.current = [];
  };

  const transitionPreviewCharacter = (next?: string) => {
    if (!next || next === previewCharacterCurrent.current) return;
    clearPreviewCharacterTimers();
    const previous = previewCharacterCurrent.current;
    previewCharacterCurrent.current = next;
    setPreviewCharacters(previous ? { previous } : {});
    previewCharacterTimers.current.push(setTimeout(() => {
      setPreviewCharacters({ current: next, previous });
    }, previous ? 240 : 50));
    if (previous) previewCharacterTimers.current.push(setTimeout(() => {
      setPreviewCharacters({ current: next });
    }, 2150));
  };

  const openPreview = (story: Story) => {
    const firstCharacter = story.preview?.blocks.find((block) => block.type === 'character')?.character;
    clearPreviewCharacterTimers();
    previewCharacterCurrent.current = undefined;
    setPreviewCharacters({});
    setPreviewStory(story);
    previewCharacterTimers.current.push(setTimeout(() => transitionPreviewCharacter(firstCharacter), 20));
  };

  const updatePreviewCharacter = (scroller: HTMLDivElement) => {
    const atEnd = scroller.scrollTop + scroller.clientHeight >= scroller.scrollHeight - 2;
    const threshold = atEnd ? scroller.scrollHeight : scroller.scrollTop + scroller.clientHeight * .42;
    const markers = Array.from(scroller.querySelectorAll<HTMLElement>('[data-character]'));
    let next = markers[0]?.dataset.character;
    for (const marker of markers) {
      if (marker.offsetTop <= threshold) next = marker.dataset.character;
    }
    transitionPreviewCharacter(next);
  };

  useEffect(() => {
    let cancelled = false;
    loadStories()
      .then((loaded) => { if (!cancelled) setStories(loaded); })
      .catch(() => { if (!cancelled) setFailed(true); });
    return () => { cancelled = true; };
  }, []);

  useEffect(() => {
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape' && previewStory) {
        setPreviewStory(null);
        return;
      }
      if (previewStory) return;
      if (event.key === 'ArrowRight' || event.key === 'PageDown') goTo(active + 1);
      if (event.key === 'ArrowLeft' || event.key === 'PageUp') goTo(active - 1);
    };
    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, [active, stories.length, previewStory]);

  useEffect(() => () => clearPreviewCharacterTimers(), []);

  return (
    <main
      className="site-shell"
      onPointerDown={(event) => { touchStart.current = event.clientX; }}
      onPointerUp={(event) => {
        if (touchStart.current === null) return;
        const distance = event.clientX - touchStart.current;
        if (Math.abs(distance) > 48) goTo(active + (distance < 0 ? 1 : -1));
        touchStart.current = null;
      }}
    >
      {locked && (
        <div className="access-gate" role="dialog" aria-modal="true" aria-labelledby="access-title">
          <form className="access-form" onSubmit={unlock}>
            <h1 id="access-title">Закрытый просмотр</h1>
            <p>Введите пароль, чтобы продолжить.</p>
            <label htmlFor="access-password">Пароль</label>
            <input
              id="access-password"
              type="password"
              value={password}
              onChange={(event) => { setPassword(event.target.value); setPasswordFailed(false); }}
              autoComplete="current-password"
              autoFocus
              aria-invalid={passwordFailed}
            />
            {passwordFailed && <p className="access-error" role="alert">Неверный пароль</p>}
            <button type="submit">Открыть</button>
          </form>
        </div>
      )}
      <div className="story-stage" aria-live="polite">
        {!stories.length && <div className="catalog-state">{failed ? 'Истории временно недоступны' : 'Загружаем истории…'}</div>}
        {stories.map((story, index) => (
          <section
            className={`story-screen ${active === index ? 'is-active' : ''}`}
            id={story.id}
            key={`${story.id}-${story.cover}`}
            aria-labelledby={`${story.id}-title`}
            aria-hidden={active !== index}
            style={{ '--accent': story.accent } as React.CSSProperties}
          >
            <div className="story-art" style={{ backgroundImage: `url(${story.cover})` }} aria-hidden="true" />
            <div className="story-shade" aria-hidden="true" />
            <div className="story-copy">
              <p className="app-name">Кострома · {String(index + 1).padStart(2, '0')}</p>
              <div className="story-meta">
                {story.genre && <p className="genre">{story.genre}</p>}
                {story.releaseStage === 'beta' && <span className="story-status" title="История находится в разработке">Бета</span>}
              </div>
              <h1 id={`${story.id}-title`}>{story.title}</h1>
              <span className="rule" aria-hidden="true" />
              {story.description && <p className="annotation">{story.description}</p>}
              {story.preview && <button className="preview-trigger" onClick={() => openPreview(story)}>Читать начало <span aria-hidden="true">→</span></button>}
              {webVersions[story.id] && <a className="preview-trigger" href={`/player/index.html?story=${encodeURIComponent(story.id)}&version=${encodeURIComponent(webVersions[story.id])}`}>Читать в браузере <span aria-hidden="true">→</span></a>}
            </div>
          </section>
        ))}
      </div>

      <div className="download-dock" aria-label="Загрузка Костромы">
        <span className="download download-unavailable" aria-disabled="true">Стабильная — позже</span>
        <a className="download download-beta" href="https://pureshechka.com/DevBuilds/Kostroma-dev.apk"><span>Попробовать новую <small>Бета</small></span><b aria-hidden="true">↓</b></a>
      </div>

      {stories.length > 1 && (
        <nav className="story-nav" aria-label="Переключение историй">
          <button className="arrow" onClick={() => goTo(active - 1)} aria-label="Предыдущая история">←</button>
          <div className="dots">
            {stories.map((story, index) => <button key={story.id} className={`dot ${active === index ? 'active' : ''}`} onClick={() => goTo(index)} aria-label={`Открыть историю «${story.title}»`} aria-current={active === index ? 'true' : undefined} />)}
          </div>
          <span className="counter">{active + 1} / {stories.length}</span>
          <button className="arrow" onClick={() => goTo(active + 1)} aria-label="Следующая история">→</button>
        </nav>
      )}

      {previewStory?.preview && (
        <div
          className="preview-backdrop"
          onClick={(event) => { if (event.target === event.currentTarget) setPreviewStory(null); }}
          onPointerDown={(event) => event.stopPropagation()}
          onPointerUp={(event) => event.stopPropagation()}
        >
          <section className="preview-modal" role="dialog" aria-modal="true" aria-labelledby="preview-title">
            <header className="preview-header">
              <div>
                <p className="preview-kicker">{previewStory.preview.subtitle || 'Начало истории'}{previewStory.preview.estimatedReadingMinutes ? ` · ${previewStory.preview.estimatedReadingMinutes} мин` : ''}</p>
                <h2 id="preview-title">{previewStory.preview.title}</h2>
              </div>
              <button className="preview-close" onClick={() => setPreviewStory(null)} aria-label="Закрыть превью">×</button>
            </header>
            <div className="preview-body">
              <div className="preview-scroll" onScroll={(event) => updatePreviewCharacter(event.currentTarget)}>
                {previewStory.preview.blocks.map((block, index) => {
                  if (block.type === 'character') return <span className="preview-character-marker" data-character={block.character} key={index} />;
                  if (block.type === 'separator') return <hr className="preview-separator" key={index} />;
                  if (block.type === 'dialogue') return <div className="preview-dialogue" key={index}><p className="preview-speaker">{block.speaker}</p><p>{block.text}</p></div>;
                  return <p className="preview-narration" key={index}>{block.text}</p>;
                })}
                {webVersions[previewStory.id]
                  ? <a className="preview-trigger" href={`/player/index.html?story=${encodeURIComponent(previewStory.id)}&version=${encodeURIComponent(webVersions[previewStory.id])}`}>Читать в браузере →</a>
                  : <p className="preview-ending">Продолжение — в приложении</p>}
              </div>
              <div className="preview-stage" data-story={previewStory.id} style={{ backgroundImage: `url(${previewStory.cover})` }} aria-live="polite">
                <span className="preview-stage-shade" aria-hidden="true" />
                {Object.entries(previewStory.preview.characters ?? {}).map(([id, character]) => {
                  const state = id === previewCharacters.current ? 'is-current' : id === previewCharacters.previous ? 'is-previous' : '';
                  return <img className={`preview-character ${state}`} src={character.image} alt={state === 'is-current' ? character.name : ''} aria-hidden={state !== 'is-current'} key={id} />;
                })}
              </div>
            </div>
          </section>
        </div>
      )}
    </main>
  );
}
