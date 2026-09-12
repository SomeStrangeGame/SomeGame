(() => {
  'use strict';
  const byId = id => document.getElementById(id);
  const canvas = byId('canvas');
  const panel = byId('panel');
  const retry = byId('retry');
  const next = byId('next');
  const back = byId('return');
  const status = byId('status');
  const progress = byId('progress');
  let unity;
  let launch;
  let alive = false;
  let busy = true;
  let failure = false;
  let nextAvailable = false;
  let returnRequested = false;
  let safeToLeave = false;

  function show(title, message, options = {}) {
    panel.hidden = false;
    byId('panel-title').textContent = title;
    byId('message').textContent = message;
    retry.hidden = !options.retry;
    next.hidden = !options.next;
    progress.hidden = !options.loading;
    retry.disabled = busy;
    next.disabled = busy;
    canvas.style.pointerEvents = 'none';
  }

  function loading(message) {
    busy = true;
    failure = false;
    nextAvailable = false;
    status.textContent = message;
    progress.removeAttribute('value');
    show(message, 'Пожалуйста, подождите. Сохранения останутся на этом устройстве.', { loading:true });
    back.disabled = true;
  }

  function error(message, saving = false) {
    failure = true;
    busy = false;
    status.textContent = saving ? 'Сохранение не завершено' : 'Чтение приостановлено';
    show(saving ? 'Не удалось сохранить прогресс' : 'Не удалось продолжить чтение', message, { retry:true });
    back.disabled = false;
    retry.focus();
  }

  function send(method, value = '') {
    try { unity.SendMessage('WebStoryPlayer', method, value); }
    catch (exception) {
      console.error(exception);
      error('Плеер не ответил. Не закрывайте страницу, если последнее действие ещё не сохранено.');
    }
  }

  function leave() {
    safeToLeave = true;
    window.location.assign('/');
  }

  back.addEventListener('click', () => {
    if (!alive) { leave(); return; }
    if (busy) return;
    returnRequested = true;
    busy = true;
    back.disabled = retry.disabled = next.disabled = true;
    status.textContent = 'Сохраняем перед выходом…';
    send('ReturnToSite');
  });
  next.addEventListener('click', () => {
    if (busy || !nextAvailable) return;
    loading('Открываем следующий эпизод…');
    send('NextEpisode');
  });
  retry.addEventListener('click', () => {
    if (busy) return;
    if (!unity || !launch) { window.location.reload(); return; }
    const returning = returnRequested;
    loading(returning ? 'Повторяем сохранение перед выходом…' : 'Повторяем запуск…');
    send(returning ? 'ReturnToSite' : 'Launch', returning ? '' : JSON.stringify(launch));
  });
  window.addEventListener('beforeunload', event => {
    if (alive && !safeToLeave) { event.preventDefault(); event.returnValue = ''; }
  });
  window.addEventListener('somegame:web-player-event', event => {
    const data = event.detail;
    if (!launch || !data || data.storyId !== launch.storyId || data.storyVersion !== launch.storyVersion) return;
    switch (data.type) {
      case 'launch_accepted': alive = true; returnRequested = false; loading('Загружаем историю…'); break;
      case 'reading_started': status.textContent = 'Открываем эпизод…'; break;
      case 'dialogue_ready':
        busy = false; failure = false; panel.hidden = true; back.disabled = false;
        canvas.style.pointerEvents = ''; status.textContent = 'Прогресс сохраняется автоматически'; break;
      case 'save_committed': status.textContent = 'Прогресс сохранён'; break;
      case 'runtime_session_stopping':
        busy = true; back.disabled = retry.disabled = next.disabled = true;
        status.textContent = 'Завершаем сохранение…'; break;
      case 'runtime_session_cancelled':
        alive = false; busy = false; back.disabled = retry.disabled = false; break;
      case 'next_episode_available':
        busy = false; nextAvailable = true; back.disabled = false; status.textContent = 'Эпизод завершён';
        show('Эпизод завершён', 'Ваши решения сохранены. Можно продолжать историю.', { next:true }); next.focus(); break;
      case 'story_completed':
        busy = false; back.disabled = false; status.textContent = 'История завершена';
        show('История завершена', 'Спасибо за чтение. Вернуться к другим историям можно кнопкой сверху.'); break;
      case 'return_to_site': leave(); break;
      case 'runtime_error':
        error(data.code === 'session_stop_failed'
          ? 'Последнее действие пока не записано. Не закрывайте и не обновляйте страницу. Освободите место на устройстве или разрешите хранение данных сайта, затем повторите попытку.'
          : 'Сохранённый прогресс не удалён. Повторите попытку. Если ошибка повторяется, не очищайте данные сайта: это удалит локальные сохранения.', data.code === 'session_stop_failed'); break;
      case 'launch_rejected':
        if (data.code !== 'session_stopping') error('Не удалось запустить историю. Повторите попытку.'); break;
    }
  });

  function localBuildPath(value) {
    return typeof value === 'string' && /^\/player\/builds\/[a-zA-Z0-9/_\-.]+$/.test(value)
      && !value.includes('..') && !value.includes('//');
  }

  async function start() {
    try {
      const query = new URLSearchParams(window.location.search);
      const id = query.get('story');
      const version = query.get('version');
      if (!id || !/^[a-z0-9]+(?:-[a-z0-9]+)*$/.test(id)) throw new Error('Invalid story');
      const response = await fetch('/player/config.json', { cache:'no-store' });
      if (!response.ok) throw new Error('Player configuration unavailable');
      const config = await response.json();
      const story = Object.hasOwn(config.stories || {}, id) ? config.stories[id] : null;
      if (config.schema !== 1 || !story || !config.player || story.version !== version
        || !/^[a-zA-Z0-9._-]+$/.test(version) || version === '.' || version === '..') throw new Error('Story is not enabled');
      const expected = `/content/stories/${id}/${version}/webgl/release.json`;
      if (story.manifestUrl !== expected) throw new Error('Invalid story manifest');
      for (const key of ['loaderUrl','dataUrl','frameworkUrl','codeUrl','streamingAssetsUrl']) {
        if (!localBuildPath(config.player[key])) throw new Error('Invalid player asset');
      }
      byId('title').textContent = story.title || 'Читать историю';
      document.title = `${story.title || 'Читать историю'} · Кострома`;
      launch = { storyId:id, storyVersion:version, manifestUrl:expected, profile:'media-free-v1', locale:'ru', returnUrl:'/' };
      const script = document.createElement('script');
      script.src = config.player.loaderUrl;
      await new Promise((resolve, reject) => { script.onload = resolve; script.onerror = reject; document.head.append(script); });
      unity = await window.createUnityInstance(canvas, {
        dataUrl:config.player.dataUrl, frameworkUrl:config.player.frameworkUrl, codeUrl:config.player.codeUrl,
        streamingAssetsUrl:config.player.streamingAssetsUrl, companyName:'SomeGame', productName:'Story Player', productVersion:'1.0',
        showBanner:(message, type) => { if (type === 'error') { console.error(message); error('Не удалось загрузить плеер. Попробуйте ещё раз.'); } },
      }, value => { progress.value = value; });
      if (!failure) send('Launch', JSON.stringify(launch));
    } catch (exception) {
      console.error(exception);
      error('Эта версия истории пока недоступна в браузере. Вернитесь к каталогу или повторите попытку позже.');
    }
  }
  start();
})();
