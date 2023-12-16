import { graphql, Link } from "gatsby";
import * as React from "react";
import { HTMLContent } from "../components/common/Content";
import Layout from "../components/layout/Layout";

// props
interface Props {
  data: {
    page: {
      html: any;
      frontmatter: {
        title: string;
        description?: string;
        keywords?: Array<string>;
      };
    };
    posts: {
      edges: Array<{
        node: {
          html: any;
          frontmatter: {
            title: string;
            publish?: string;
          };
        };
      }>;
    };
  };
}

interface PostProps {
  title: string;
  content: string;
  publish?: string;
}

const Post = ({ title, content, publish }: PostProps) => {
  return (
    <>
      <article>
        <h1 className="is-size-3">{title}</h1>
        <p className="has-text-weight-bold pb-2">{publish}</p>
        <HTMLContent
          content={content}
          className={"markdown content"}
        ></HTMLContent>
      </article>
      <hr />
    </>
  );
};

// markup
const BlogTemplate = (props: Props) => {
  return (
    <Layout
      seo={{
        title: props.data.page.frontmatter.title,
        description: props.data.page.frontmatter.description,
        keywords: props.data.page.frontmatter.keywords,
      }}
      className="home"
    >
      <div className="default-banner">
        <div className="extra">
          <img src={`/img/extra-1.svg`} />
        </div>
      </div>
      <div className="container my-6">
        <HTMLContent
          content={props.data.page.html}
          className={"markdown content"}
        ></HTMLContent>
        <hr />
        <div className="markdown">
          {props.data.posts.edges.map((edge, i) => (
            <Post
              key={`post${i}`}
              title={edge.node.frontmatter.title}
              content={edge.node.html}
              publish={edge.node.frontmatter.publish}
            />
          ))}
        </div>
      </div>
    </Layout>
  );
};

export default BlogTemplate;

// graphQL queries
export const pageQuery = graphql`
    query BlogTemplate($id: String!) {
      posts: allMarkdownRemark(
        sort: {frontmatter: {publish: DESC}}
        filter: {frontmatter: {templateKey: {eq: "blog-template"}}}
      ) {
        edges {
          node {
            html
            frontmatter {
              title
              publish(formatString: "MMMM DD, YYYY")
            }
          }
        }
      }
      page: markdownRemark(id: {eq: $id}) {
        html
        frontmatter {
          title
          description
          keywords
        }
      }
    }
`;
