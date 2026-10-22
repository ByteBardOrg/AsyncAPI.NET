import React from 'react';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import styles from './index.module.css';

const paths = [
  {number: '01', title: 'Use the library', text: 'Install the packages, read AsyncAPI documents, and serialize them back to JSON or YAML.', to: '/docs/guides/getting-started'},
  {number: '02', title: 'Extend the library', text: 'Adapt parsing and validation to the needs of your application.', to: '/docs/guides/extensions'},
  {number: '03', title: 'Bindings', text: 'Explore protocol-specific support for common messaging platforms.', to: '/docs/bindings'},
];

export default function Home() {
  return <Layout title="AsyncAPI.NET" description="A .NET object model and toolkit for AsyncAPI documents.">
    <main className={styles.page}>
      <section className={styles.hero}>
        <div className={styles.orbit} aria-hidden="true"><span /><span /><span /></div>
        <div className={styles.heroCopy}>
          <p className={styles.eyebrow}><i /> OPEN-SOURCE .NET TOOLKIT</p>
          <h1>AsyncAPI,<br /><em>at home</em> in .NET.</h1>
          <p className={styles.lede}>A practical object model for building, reading, and writing AsyncAPI documents with .NET.</p>
          <div className={styles.actions}><Link className={styles.primary} to="/docs/guides/getting-started">Get started <span>↗</span></Link><Link className={styles.secondary} to="https://github.com/ByteBardOrg/AsyncAPI.NET">View on GitHub</Link></div>
        </div>
        <div className={styles.code} aria-label="Example AsyncAPI document"><div className={styles.codeHead}><span>asyncapi.yml</span><span>● ● ●</span></div><pre><code><span className={styles.comment}># Describe your event-driven API</span>{'\n'}<b>asyncapi</b>: <span className={styles.green}>3.0.0</span>{'\n'}<b>info</b>:{'\n'}  title: <span className={styles.orange}>Orders API</span>{'\n'}  version: <span className={styles.orange}>1.0.0</span>{'\n'}<b>channels</b>:{'\n'}  orders/created:{'\n'}    address: <span className={styles.orange}>orders/created</span>{'\n'}<b>operations</b>:{'\n'}  receiveOrder:{'\n'}    action: <span className={styles.green}>receive</span></code></pre><div className={styles.codeFoot}><span>↓</span> read · model · write</div></div>
        <div className={styles.heroNote}>SPECIFICATION<br /><strong>2.6 <span>↗</span> 3.1</strong></div>
      </section>
      <section className={styles.paths}><div className={styles.sectionHead}><p className={styles.eyebrow}>A CLEAR WAY IN</p><h2>Start with what<br />you need to do.</h2></div><div className={styles.cards}>{paths.map((item) => <Link key={item.number} to={item.to} className={styles.card}><span className={styles.number}>{item.number}</span><span className={styles.arrow}>↗</span><h3>{item.title}</h3><p>{item.text}</p><span className={styles.cardCta}>Explore <span>→</span></span></Link>)}</div></section>
      <section className={styles.note}><p>AsyncAPI.NET supports the current 2.x and 3.x specifications. It reads 2.6 documents into the V3 model and can write them back as 2.6; V3 output is currently 3.1.0.</p><Link to="/docs/guides/version-conversion">Learn about document versions <span>↗</span></Link></section>
    </main>
  </Layout>;
}
