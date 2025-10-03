### UI

chat-manager-max-message-length = Ваше сообщение превышает лимит в { $maxMessageLength } символов
chat-manager-ooc-chat-enabled-message = OOC чат был включен.
chat-manager-ooc-chat-disabled-message = OOC чат был отключен.
chat-manager-looc-chat-enabled-message = LOOC чат был включен.
chat-manager-looc-chat-disabled-message = LOOC чат был отключен.
chat-manager-dead-looc-chat-enabled-message = Мёртвые игроки теперь могут говорить в LOOC.
chat-manager-dead-looc-chat-disabled-message = Мёртвые игроки больше не могут говорить в LOOC.
chat-manager-crit-looc-chat-enabled-message = Игроки в критическом состоянии теперь могут использовать LOOC.
chat-manager-crit-looc-chat-disabled-message = Игроки в критическом состоянии теперь не могут использовать LOOC.
chat-manager-admin-ooc-chat-enabled-message = Админ OOC чат был включен.
chat-manager-admin-ooc-chat-disabled-message = Админ OOC чат был выключен.

chat-manager-max-message-length-exceeded-message = Ваше сообщение превышает лимит в { $limit } символов
chat-manager-no-headset-on-message = У вас нет гарнитуры!
chat-manager-no-radio-key = Не указан ключ канала!
chat-manager-no-such-channel =  Нет канала с ключем '{$key}'!
chat-manager-whisper-headset-on-message = Вы не можете шептать в радио!

chat-manager-language-prefix = ({ $language }){" "}

chat-manager-server-wrap-message = СЕРВЕР: { $message }
chat-manager-sender-announcement = Central Command
chat-manager-sender-announcement-wrap-message = [font size=14][bold]Объявление {$sender}:[/font][font size=12]
                                                {$message}[/bold][/font]

# For the message in double quotes, the font/color/bold/italic elements are repeated twice, outside the double quotes and inside.
# The outside elements are for formatting the double quotes, and the inside elements are for formatting the text in speech bubbles ([BubbleContent]).
chat-manager-entity-say-wrap-message = [bold][BubbleHeader][Name][font size=11][color={$color}][bold]{$language}[/bold][/color][/font]{$entityName}[/Name][/BubbleHeader][/bold] [italic]{$verb}[/italic], [font="{$fontType}" size={$fontSize}]"[BubbleContent]{$message}[/BubbleContent]"[/font]
chat-manager-entity-say-bold-wrap-message = [bold][BubbleHeader][Name][font size=11][color={$color}][bold]{$language}[/bold][/color][/font]{$entityName}[/Name][/BubbleHeader][/bold] {$verb}, [font="{$fontType}" size={$fontSize}]"[bold][BubbleContent]{$message}[/BubbleContent][/bold]"[/font]

chat-manager-entity-whisper-wrap-message = [font size=11][italic][BubbleHeader][Name][font size=10][color={$color}][bold]{$language}[/bold][/color][/font]{$entityName}[/Name][/BubbleHeader] шепчет,"[BubbleContent]{$message}[/BubbleContent]"[/italic][/font]
chat-manager-entity-whisper-unknown-wrap-message = [font size=11][italic][BubbleHeader][font size=10][color={$color}][bold]{$language}[/bold][/color][/font]Некто[/BubbleHeader] шепчет, "[BubbleContent]{$message}[/BubbleContent]"[/italic][/font]

# THE() is not used here because the entity and its name can technically be disconnected if a nameOverride is passed...
chat-manager-entity-me-wrap-message = { $entityName } { $message }

chat-manager-entity-looc-wrap-message = LOOC: {$entityName}: [BubbleContent]{$message}[/BubbleContent]
chat-manager-send-ooc-wrap-message = OOC: [bold]{$playerName}:[/bold] {$message}
chat-manager-send-ooc-patron-wrap-message = OOC: [color={$patronColor}]{$playerName}[/color]: {$message}

chat-manager-send-dead-chat-wrap-message = {$deadChannelName}: [bold][BubbleHeader]{$playerName}[/BubbleHeader]:[/bold] [BubbleContent]{$message}[/BubbleContent]
chat-manager-send-admin-dead-chat-wrap-message = {$adminChannelName}: [bold]([BubbleHeader]{$userName}[/BubbleHeader]):[/bold] [BubbleContent]{$message}[/BubbleContent]
chat-manager-send-admin-chat-wrap-message = {$adminChannelName}: [bold]{$playerName}:[/bold] {$message}
chat-manager-send-admin-announcement-wrap-message = [bold]{$adminChannelName}: {$message}[/bold]

chat-manager-send-hook-ooc-wrap-message = OOC: [bold](D){$senderName}:[/bold] {$message}

chat-manager-dead-channel-name = МЁРТВЫЕ
chat-manager-admin-channel-name = АДМИН
chat-manager-rate-limited = Вы отправляете сообщения слишком часто!
chat-manager-rate-limit-admin-announcement = Игрок { $player } превысил рейт-лимит чата.

## Speech verbs for chat

chat-manager-send-empathy-chat-wrap-message = {$source}: {$message}

chat-manager-send-cult-chat-wrap-message = [bold]\[{ $channelName }\] [BubbleHeader]{ $player }[/BubbleHeader]:[/bold] [BubbleContent]{ $message }[/BubbleContent]
chat-manager-cult-channel-name = Blood Cult

## Speech verbs for chat

chat-speech-verb-suffix-exclamation = !
chat-speech-verb-suffix-exclamation-strong = !!
chat-speech-verb-suffix-question = ?
chat-speech-verb-suffix-stutter = -
chat-speech-verb-suffix-mumble = ..

chat-speech-verb-name-none = Отсутствует
chat-speech-verb-name-default = По умолчанию
chat-speech-verb-default = говорит
chat-speech-verb-name-exclamation = Восклицание
chat-speech-verb-exclamation = восклицает
chat-speech-verb-name-exclamation-strong = Крик
chat-speech-verb-exclamation-strong = кричит
chat-speech-verb-name-question = Вопрос
chat-speech-verb-question = спрашивает
chat-speech-verb-name-stutter = Заикание
chat-speech-verb-stutter = заикается
chat-speech-verb-name-mumble = Бормотание
chat-speech-verb-mumble = бормочет

chat-speech-verb-name-arachnid = Арахнид
chat-speech-verb-insect-1 = стрекочет
chat-speech-verb-insect-2 = чирикает
chat-speech-verb-insect-3 = щелкает

chat-speech-verb-name-moth = Мотылёк
chat-speech-verb-winged-1 = трепещет
chat-speech-verb-winged-2 = жужжит
chat-speech-verb-winged-3 = похлопывает

chat-speech-verb-name-slime = Слайм
chat-speech-verb-slime-1 = бормочет
chat-speech-verb-slime-2 = булькает
chat-speech-verb-slime-3 = хлюпает

chat-speech-verb-name-plant = Диона
chat-speech-verb-plant-1 = шуршит
chat-speech-verb-plant-2 = скрипит
chat-speech-verb-plant-3 = трещит

chat-speech-verb-name-robotic = Роботизированный
chat-speech-verb-robotic-1 = утверждает
chat-speech-verb-robotic-2 = бипает
chat-speech-verb-robotic-3 = пикает

chat-speech-verb-name-reptilian = Рептилоид
chat-speech-verb-reptilian-1 = шипит
chat-speech-verb-reptilian-2 = фыркает
chat-speech-verb-reptilian-3 = пыхтит

chat-speech-verb-name-skeleton = Скелет / Плазмочеловек
chat-speech-verb-skeleton-1 = гремит
chat-speech-verb-skeleton-2 = ребрами
chat-speech-verb-skeleton-3 = костями
chat-speech-verb-skeleton-4 = стучит
chat-speech-verb-skeleton-5 = трещит

chat-speech-verb-name-vox = Вокс
chat-speech-verb-vox-1 = верещит
chat-speech-verb-vox-2 = визжит
chat-speech-verb-vox-3 = каркает

chat-speech-verb-name-oni = Они
chat-speech-verb-oni-1 = ворчит
chat-speech-verb-oni-2 = ревет
chat-speech-verb-oni-3 = гудит
chat-speech-verb-oni-4 = рокочет

chat-speech-verb-name-canine = Псовый
chat-speech-verb-canine-1 = гавкает
chat-speech-verb-canine-2 = лает
chat-speech-verb-canine-3 = воет

chat-speech-verb-name-small-mob = Мышь
chat-speech-verb-small-mob-1 = пищит
chat-speech-verb-small-mob-2 = пискает

chat-speech-verb-name-large-mob = Карп
chat-speech-verb-large-mob-1 = рычит
chat-speech-verb-large-mob-2 = урчит

chat-speech-verb-name-monkey = Обезьяна
chat-speech-verb-monkey-1 = кричит
chat-speech-verb-monkey-2 = визжит

chat-speech-verb-name-cluwne = Клуни
chat-speech-verb-cluwne-1 = хихикает
chat-speech-verb-cluwne-2 = гогочет
chat-speech-verb-cluwne-3 = смеется

chat-speech-verb-name-parrot = Попугай
chat-speech-verb-parrot-1 = вскрикивает
chat-speech-verb-parrot-2 = чирикает
chat-speech-verb-parrot-3 = щебечет

chat-speech-verb-name-ghost = Призрак
chat-speech-verb-ghost-1 = жалуется
chat-speech-verb-ghost-2 = дышит
chat-speech-verb-ghost-3 = мычит
chat-speech-verb-ghost-4 = бормочет

chat-speech-verb-name-electricity = Электричество
chat-speech-verb-electricity-1 = потрескивает
chat-speech-verb-electricity-2 = гудит
chat-speech-verb-electricity-3 = скрипит

chat-speech-verb-name-marish = Марсианин
chat-speech-verb-marish = марсиански говорит

chat-speech-verb-name-supermatter = Суперматерия
chat-speech-verb-supermatter = излагает

chat-speech-verb-name-psychomantic = Психомантический
chat-speech-verb-Psychomantic-1 = резонирует
chat-speech-verb-Psychomantic-2 = проецирует
chat-speech-verb-Psychomantic-3 = внушает
chat-speech-verb-Psychomantic-4 = излучает
