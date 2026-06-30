var v=Object.defineProperty;var f=(e,r,s)=>r in e?v(e,r,{enumerable:!0,configurable:!0,writable:!0,value:s}):e[r]=s;var i=(e,r,s)=>f(e,typeof r!="symbol"?r+"":r,s);import{j as o}from"./jsx-runtime-D_zvdyIk.js";function x({error:e,onRetry:r}){return o.jsx("div",{className:"error-message",children:o.jsxs("div",{className:"error-message__box",children:[o.jsxs("div",{className:"error-message__title",children:[o.jsx("span",{"aria-hidden":"true",children:"⚠️"}),"データの取得に失敗しました"]}),o.jsx("div",{className:"error-message__detail",children:e.message}),r&&o.jsx("button",{className:"error-message__retry",type:"button",onClick:r,children:"再試行"})]})})}x.__docgenInfo={description:"",methods:[],displayName:"ErrorMessage",props:{error:{required:!0,tsType:{name:"Error"},description:""},onRetry:{required:!1,tsType:{name:"signature",type:"function",raw:"() => void",signature:{arguments:[],return:{name:"void"}}},description:""}}};class _ extends Error{constructor(s,d){super(`API error: ${s} ${d}`);i(this,"status");i(this,"statusText");this.name="TopologyApiError",this.status=s,this.statusText=d}}const N={title:"Components/ErrorMessage",component:x,tags:["autodocs"],decorators:[e=>o.jsx("div",{style:{width:"600px",height:"300px"},children:o.jsx(e,{})})]},t={args:{error:new _(404,"Not Found"),onRetry:void 0}},a={args:{error:new TypeError("Failed to fetch"),onRetry:void 0}},n={args:{error:new _(500,"Internal Server Error"),onRetry:()=>alert("再試行しました")}};var c,p,m;t.parameters={...t.parameters,docs:{...(c=t.parameters)==null?void 0:c.docs,source:{originalSource:`{
  args: {
    error: new TopologyApiError(404, 'Not Found'),
    onRetry: undefined
  }
}`,...(m=(p=t.parameters)==null?void 0:p.docs)==null?void 0:m.source}}};var l,u,g;a.parameters={...a.parameters,docs:{...(l=a.parameters)==null?void 0:l.docs,source:{originalSource:`{
  args: {
    error: new TypeError('Failed to fetch'),
    onRetry: undefined
  }
}`,...(g=(u=a.parameters)==null?void 0:u.docs)==null?void 0:g.source}}};var y,h,E;n.parameters={...n.parameters,docs:{...(y=n.parameters)==null?void 0:y.docs,source:{originalSource:`{
  args: {
    error: new TopologyApiError(500, 'Internal Server Error'),
    onRetry: () => alert('再試行しました')
  }
}`,...(E=(h=n.parameters)==null?void 0:h.docs)==null?void 0:E.source}}};const R=["ApiError","NetworkError","WithRetry"];export{t as ApiError,a as NetworkError,n as WithRetry,R as __namedExportsOrder,N as default};
