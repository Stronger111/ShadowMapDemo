# ShadowMapDemo
Unity 实现光照阴影
首先说一个ShadowMap的原理，Shadow Map示意图如下

三个步骤：
（1）以光源视角渲染场景，得到深度贴图（DepthMap），并存储为texture
（2）实际相机渲染物体，将物体从世界坐标转换到光源视角下，与深度纹理对比数据获得阴影信息
（3）根据阴影信息渲染场景以及阴影
b：阴影的处理有很多方式,获得一个比较好的阴影还是需要很多优化的。
参考：
https://github.com/Richbabe/ShadowMap_Unity
          Unity基础(5)Shadow Map 概述
          Unity移动端动态阴影总结
