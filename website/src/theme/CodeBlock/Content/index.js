/**
 * Swizzled from @docusaurus/theme-classic to highlight code with Shiki instead of Prism. The Prism-shaped
 * line and token props are kept, so the theme's Line component still renders line numbers and highlighted lines.
 */
import React, {useMemo} from 'react';
import clsx from 'clsx';
import {useCodeBlockContext} from '@docusaurus/theme-common/internal';
import Line from '@theme/CodeBlock/Line';
import {highlight} from './shiki';
import styles from './styles.module.css';

const Pre = React.forwardRef((props, ref) => {
  return (
    <pre
      ref={ref}
      /* eslint-disable-next-line jsx-a11y/no-noninteractive-tabindex */
      tabIndex={0}
      {...props}
      className={clsx(props.className, styles.codeBlock, 'thin-scrollbar')}
    />
  );
});

function Code(props) {
  const {metadata} = useCodeBlockContext();
  return (
    <code
      {...props}
      className={clsx(
        props.className,
        styles.codeBlockLines,
        metadata.lineNumbersStart !== undefined && styles.codeBlockLinesWithNumbering,
      )}
      style={{
        ...props.style,
        counterReset: metadata.lineNumbersStart === undefined ? undefined : `line-count ${metadata.lineNumbersStart - 1}`,
      }}
    />
  );
}

function getLineProps({className}) {
  return {className: clsx('token-line', className)};
}

function getTokenProps({token}) {
  return {className: 'token shiki-token', style: token.style, children: token.content};
}

export default function CodeBlockContent({className}) {
  const {metadata, wordWrap} = useCodeBlockContext();
  const {code, language, lineNumbersStart, lineClassNames} = metadata;
  const lines = useMemo(
    () => highlight(code, language).map((line) => line.map((token) => ({content: token.content, style: token.htmlStyle}))),
    [code, language],
  );

  return (
    <Pre ref={wordWrap.codeBlockRef} className={clsx(className, `language-${language}`)}>
      <Code>
        {lines.map((line, i) => (
          <Line
            key={i}
            line={line}
            getLineProps={getLineProps}
            getTokenProps={getTokenProps}
            classNames={lineClassNames[i]}
            showLineNumbers={lineNumbersStart !== undefined}
          />
        ))}
      </Code>
    </Pre>
  );
}
